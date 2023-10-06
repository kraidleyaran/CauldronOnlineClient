using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Waypoint Trait", menuName = "Ancible Tools/Traits/Interactable/Waypoint")]
    public class WaypointTrait : InteractableTrait
    {
        protected internal override string _actionText => "Bind";

        [SerializeField] private WorldVector2Int _offset = WorldVector2Int.Zero;

        private WorldVector2Int _position = WorldVector2Int.Zero;

        protected internal override void Interact()
        {
            _controller.gameObject.SendMessageTo(FullHealMessage.INSTANCE, ObjectManager.Player);
            DataController.CurrentCharacter.Zone = WorldZoneManager.CurrentZone.name;
            DataController.CurrentCharacter.Position = _position;
            DataController.SavePlayerData();
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetWorldPositionMessage>(SetWorldPosition, _instanceId);
        }

        private void SetWorldPosition(SetWorldPositionMessage msg)
        {
            _position = msg.Position + _offset;
            var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
            updateWorldPositionMsg.Position = msg.Position;
            _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWorldPositionMsg);
        }
    }
}