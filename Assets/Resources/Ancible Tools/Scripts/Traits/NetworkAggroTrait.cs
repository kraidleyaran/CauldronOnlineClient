using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Aggro Trait", menuName = "Ancible Tools/Traits/Network/Network Aggro")]
    public class NetworkAggroTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _baseHitbox;

        private HitboxController _hitboxController;

        private bool _aggrod = false;

        private GameObject _currentTarget = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_baseHitbox, CollisionLayerFactory.MonsterAggro);

            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);

            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _instanceId);
            _controller.gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObject, _instanceId);

            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAggroRangeMessage>(SetAggroRange, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCurrentAggroTargetMessage>(SetCurrentAggroTarget, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryTargetMessage>(QueryTarget, _instanceId);
        }

        private void SetAggroRange(SetAggroRangeMessage msg)
        {
            _hitboxController.SetSize(msg.AggroRange);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (!_aggrod)
            {
                _aggrod = true;
                var objId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                ClientController.SendToServer(new ClientAggroMessage{AggrodObjectId = objId});
            }
        }

        private void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            if (_aggrod)
            {
                _aggrod = false;
                var objId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                ClientController.SendToServer(new ClientAggroMessage { AggrodObjectId = objId, Remove = true});
            }
        }

        private void SetCurrentAggroTarget(SetCurrentAggroTargetMessage msg)
        {
            _currentTarget = msg.Target;
        }

        private void QueryTarget(QueryTargetMessage msg)
        {
            msg.DoAfter.Invoke(_currentTarget);
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);

                _hitboxController = null;
            }
            base.Destroy();
        }
    }

}