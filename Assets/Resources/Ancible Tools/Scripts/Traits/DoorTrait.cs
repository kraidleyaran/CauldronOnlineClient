using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Door Trait", menuName = "Ancible Tools/Traits/Interactable/Door")]
    public class DoorTrait : InteractableTrait
    {
        protected internal override string _actionText => "Open";

        [SerializeField] private Hitbox.Hitbox _trappedHitbox;

        private HitboxController _trappedHitboxController = null;
        private object _trappedReceiver = new object();
        private WorldVector2Int _trappedSpawn = WorldVector2Int.Zero;

        private bool _open = false;

        private bool _playerInsideDoor = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _trappedHitboxController = _controller.gameObject.SetupHitbox(_trappedHitbox, CollisionLayerFactory.ZoneTransfer);
            SubscribeToMessages();
        }

        protected internal override void Interact()
        {
            if (!_open)
            {
                var targetId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(targetId))
                {
                    ClientController.SendToServer(new ClientDoorCheckMessage{TargetId = targetId, Tick = TickController.ServerTick});
                }
            }
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupDoorMessage>(SetupDoor, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDoorStateMessage>(SetDoorState, _instanceId);
            _trappedReceiver.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithTrappedObject, _instanceId);
        }

        protected internal override void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            base.EnterCollisionWithObject(msg);
            if (msg.Object == ObjectManager.Player)
            {
                _playerInsideDoor = true;
            }
        }

        protected internal override void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            base.ExitCollisionWithObject(msg);
            if (msg.Object == ObjectManager.Player)
            {
                _playerInsideDoor = false;
            }
            
        }

        private void SetupDoor(SetupDoorMessage msg)
        {
            _open = msg.Open;
            _trappedSpawn = msg.TrappedSpawnPosition;
            _controller.transform.parent.SetLocalRotation(msg.Rotation);
            _controller.transform.parent.gameObject.SetActive(!_open);
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }

        private void SetDoorState(SetDoorStateMessage msg)
        {
            _open = msg.Open;
            if (!_open)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                addTraitToUnitMsg.Trait = TraitFactory.AppearanceFx;
                _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(addTraitToUnitMsg);
                if (_playerInsideDoor)
                {
                    var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                    setWorldPositionMsg.Position = _trappedSpawn;
                    _controller.gameObject.SendMessageTo(setWorldPositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setWorldPositionMsg);

                    ClientController.SendToServer(new ClientTeleportMessage{Position = _trappedSpawn});
                }
            }
            _controller.transform.parent.gameObject.SetActive(!_open);

        }

        private void EnterCollisionWithTrappedObject(EnterCollisionWithObjectMessage msg)
        {
            if (msg.Object == ObjectManager.Player)
            {
                var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                setWorldPositionMsg.Position = _trappedSpawn;
                _controller.gameObject.SendMessageTo(setWorldPositionMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(setWorldPositionMsg);

                ClientController.SendToServer(new ClientTeleportMessage{Position = _trappedSpawn, Tick = TickController.ServerTick});
            }
        }

        public override void Destroy()
        {
            _trappedReceiver.UnsubscribeFromAllMessages();
            _trappedReceiver = null;
            _trappedHitboxController.Destroy();
            Destroy(_trappedHitboxController.gameObject);
            base.Destroy();
        }
    }
}