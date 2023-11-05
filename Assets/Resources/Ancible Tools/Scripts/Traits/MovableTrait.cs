using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Movable Trait", menuName = "Ancible Tools/Traits/Interactable/Movable")]
    public class MovableTrait : InteractableTrait
    {
        protected internal override string _actionText => "Grab";

        private int _moveSpeed = 0;
        private GameObject _owner = null;
        private bool _colliding = false;
        private WorldOffset _offset;

        private HitboxController _horizontalHitboxController = null;
        private MovableAxis _axis = MovableAxis.Horizontal;

        public override void SetupController(TraitController controller)
        {
            _horizontalHitboxController = Instantiate(_hitbox.Controller, controller.transform.parent);
            _horizontalHitboxController.Setup(CollisionLayerFactory.Interaction);
            _horizontalHitboxController.AddSubscriber(_horizontalHitboxController.gameObject);
            base.SetupController(controller);
        }

        protected internal override void Interact()
        {
            if (!_owner)
            {
                var id = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(id))
                {
                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Interaction;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(setUnitStateMsg);

                    _owner = ObjectManager.Player;
                    var setMovableMsg = MessageFactory.GenerateSetMovableMsg();
                    setMovableMsg.Movable = _controller.transform.parent.gameObject;
                    setMovableMsg.MoveSpeed = _moveSpeed;
                    setMovableMsg.Axis = _axis;
                    setMovableMsg.Id = id;
                    setMovableMsg.Offset = _offset;
                    _controller.gameObject.SendMessageTo(setMovableMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(setMovableMsg);
                }

            }
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupMovableMessage>(SetupMovable, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetOwnerMessage>(SetOwner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReleaseMovableMessage>(ReleaseMovable, _instanceId);
            _horizontalHitboxController.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(HorizontalEnterCollisionWithObject, _instanceId);
            _horizontalHitboxController.gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObject, _instanceId);
        }

        protected internal override void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (!_colliding)
            {
                base.EnterCollisionWithObject(msg);
                _axis = MovableAxis.Vertical;
                _colliding = true;
            }

        }

        private void HorizontalEnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (!_colliding)
            {
                base.EnterCollisionWithObject(msg);
                _axis = MovableAxis.Horizontal;
                _colliding = true;
            }

        }

        protected internal override void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            base.ExitCollisionWithObject(msg);
            _colliding = false;
        }

        private void SetupMovable(SetupMovableMessage msg)
        {
            _moveSpeed = msg.MoveSpeed;
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
            _horizontalHitboxController.transform.SetLocalScaling(msg.Horizontal.Size.ToVector());
            _horizontalHitboxController.transform.SetLocalPosition(msg.Horizontal.Offset.ToWorldVector());
            _offset = msg.Offset;
        }

        private void ReleaseMovable(ReleaseMovableMessage msg)
        {
            if (_owner && _owner == ObjectManager.Player)
            {
                _owner = null;
            }
        }

        private void SetOwner(SetOwnerMessage msg)
        {
            if (_owner && _owner == ObjectManager.Player)
            {
                _controller.gameObject.SendMessageTo(ReleaseMovableMessage.INSTANCE, ObjectManager.Player);
            }
            _owner = msg.Owner;
        }

        public override void Destroy()
        {
            _horizontalHitboxController.gameObject.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            _horizontalHitboxController.RemoveSubscriber(_horizontalHitboxController.gameObject);
            Destroy(_horizontalHitboxController.gameObject);
            _horizontalHitboxController = null;
            base.Destroy();
        }
    }
}