using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using DG.Tweening;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Hurtbox Trait", menuName = "Ancible Tools/Traits/Network/Network Hurtbox")]
    public class NetworkHurtboxTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox = null;
        [SerializeField] private CollisionLayer _collisionLayer;

        private HitboxController _hitboxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, _collisionLayer);

            var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
            registerCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
            MessageFactory.CacheMessage(registerCollisionMsg);

            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHurtboxSizeMessage>(SetHurtboxSize, _instanceId);
        }

        private void SetHurtboxSize(SetHurtboxSizeMessage msg)
        {
            _hitboxController.transform.SetLocalScaling(msg.Size.ToVector());
            _hitboxController.transform.localPosition = msg.Offset.ToWorldVector();
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionmsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionmsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionmsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionmsg);
            }

            _hitboxController = null;
            base.Destroy();
        }
    }
}