using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "World Position Trait", menuName = "Ancible Tools/Traits/Network/World Position")]
    public class WorldPositionTrait : Trait
    {
        private WorldVector2Int _worldPosition = WorldVector2Int.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetWorldPositionMessage>(SetWorldPosition, _instanceId);
        }

        private void SetWorldPosition(SetWorldPositionMessage msg)
        {
            _worldPosition = msg.Position;

            var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
            updateWorldPositionMsg.Position = _worldPosition;
            _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWorldPositionMsg);
        }
    }
}