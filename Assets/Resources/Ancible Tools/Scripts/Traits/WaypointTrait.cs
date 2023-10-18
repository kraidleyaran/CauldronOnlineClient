using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Waypoint Trait", menuName = "Ancible Tools/Traits/Interactable/Waypoint")]
    public class WaypointTrait : InteractableTrait
    {
        protected internal override string _actionText => "Interact";

        [SerializeField] private WorldVector2Int _offset = WorldVector2Int.Zero;

        private WorldVector2Int _position = WorldVector2Int.Zero;

        private UiWaypointWindow _waypointWindow = null;

        protected internal override void Interact()
        {
            _controller.gameObject.SendMessageTo(FullHealMessage.INSTANCE, ObjectManager.Player);
            DataController.SavePlayerData();
            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
            setUnitStateMsg.State = UnitState.Interaction;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                _waypointWindow = UiWindowManager.OpenWindow(UiController.Waypoint);
                _waypointWindow.Setup(_controller.transform.parent.gameObject, _position);
            }));


        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetWorldPositionMessage>(SetWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<WaypointWindowClosedMessage>(WaypointWindowClosed, _instanceId);
        }

        private void SetWorldPosition(SetWorldPositionMessage msg)
        {
            _position = msg.Position + _offset;
            var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
            updateWorldPositionMsg.Position = msg.Position;
            _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWorldPositionMsg);
        }

        private void WaypointWindowClosed(WaypointWindowClosedMessage msg)
        {
            _waypointWindow = null;
            var travelled = msg.Travelling;
            _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                if (!travelled)
                {
                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Active;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitStateMsg);
                }
            }));
        }
    }
}