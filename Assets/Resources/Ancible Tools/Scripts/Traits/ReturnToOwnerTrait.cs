using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Return to Owner Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Return to Owner")]
    public class ReturnToOwnerTrait : Trait
    {
        [SerializeField] private int _moveSpeed = 1;
        [SerializeField] private int _detectDistance = 16;
        [SerializeField] private Vector2Int _ownerOffset = new Vector2Int(0, 8);

        private GameObject _owner = null;

        private Vector2Int _direction = Vector2Int.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _controller.gameObject.SendMessageTo(ReturningToOwnerMessage.INSTANCE, _controller.transform.parent.gameObject);
            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = owner => _owner = owner;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryOwnerMsg);
            if (!_owner)
            {
                ObjectManager.DestroyNetworkObject(_controller.transform.parent.gameObject);
                return;
            }

            var worldPos = _controller.transform.parent.position.ToVector2().ToWorldPosition();
            var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
            setWorldPositionMsg.Position = worldPos;
            _controller.gameObject.SendMessageTo(setWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setWorldPositionMsg);

            UpdateOwnerDirection();
            SubscribeToMessage();
        }

        private void UpdateOwnerDirection()
        {
            var offset = _ownerOffset.ToVector2(true);
            var ownerDirection = _owner.transform.position.ToVector2() + offset - _controller.transform.parent.position.ToVector2();
            if (ownerDirection != _direction)
            {
                var setProjectileDirectionMsg = MessageFactory.GenerateSetProjectileDirectionMsg();
                setProjectileDirectionMsg.Direction = ownerDirection;
                _controller.gameObject.SendMessageTo(setProjectileDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setProjectileDirectionMsg);
            }
        }

        private void SubscribeToMessage()
        {
            _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
            _owner.SubscribeWithFilter<UpdateWorldPositionMessage>(OwnerUpdateWorldPosition, _instanceId);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_owner)
            {
                var diff = _owner.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2();
                var distance = diff.magnitude;
                if (distance <= _detectDistance * DataController.Interpolation)
                {
                    var projectileReturnedMsg = MessageFactory.GenerateProjectileReturnedMsg();
                    projectileReturnedMsg.Projectile = _controller.transform.parent.gameObject;
                    _controller.gameObject.SendMessageTo(projectileReturnedMsg, _owner);
                    MessageFactory.CacheMessage(projectileReturnedMsg);
                    _owner.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                    ObjectManager.DestroyNetworkObject(_controller.transform.parent.gameObject);
                }
            }
            else
            {
                ObjectManager.DestroyNetworkObject(_controller.transform.parent.gameObject);
            }
            
        }

        private void UpdateDirection(UpdateDirectionMessage msg)
        {
            _direction = msg.Direction;
            //var ownerDirection = (_owner.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).ToDirection();
            //if (ownerDirection != _direction)
            //{
            //    var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
            //    setDirectionMsg.Direction = ownerDirection;
            //    _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
            //    MessageFactory.CacheMessage(setDirectionMsg);
            //}
        }

        private void OwnerUpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            UpdateOwnerDirection();
        }

        public override void Destroy()
        {
            if (_owner)
            {
                _owner.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                _owner = null;
            }
            base.Destroy();
        }
    }
}