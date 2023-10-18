using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Zone Transition Trait", menuName = "Ancible Tools/Traits/Interactable/Zone Transition")]
    public class ZoneTransitionTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox = null;

        private HitboxController _hitboxController;
        private WorldZone _zone = null;
        private WorldVector2Int _position = WorldVector2Int.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.ZoneTransfer);
            _hitboxController.AddSubscriber(_controller.gameObject);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupZoneTransitionMessage>(SetupZoneTransition, _instanceId);
            
        }

        private void SetupZoneTransition(SetupZoneTransitionMessage msg)
        {
            var zone = WorldZoneManager.GetZoneByName(msg.Zone);
            if (zone)
            {
                _zone = zone;
                _position = msg.Position;
                _controller.transform.parent.SetLocalRotation(msg.Rotation);
            }
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (msg.Object == ObjectManager.Player)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);

                ClientController.TransferPlayer(_zone, _position);
            }
        }
    }
}