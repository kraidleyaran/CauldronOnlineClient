using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Recall Trait", menuName = "Ancible Tools/Traits/General/Recall")]
    public class RecallTrait : Trait
    {
        public override bool ApplyOnClient => false;

        private WorldZone _zone = null;
        private WorldVector2Int _position = WorldVector2Int.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (_controller.transform.parent.gameObject == ObjectManager.Player)
            {
                _controller.gameObject.SendMessageTo(RemoveActiveRecallMessage.INSTANCE, _controller.transform.parent.gameObject);
                var zone = WorldZoneManager.GetZoneByName(DataController.CurrentCharacter.Zone);
                if (zone)
                {
                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Interaction;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitStateMsg);

                    _zone = WorldZoneManager.CurrentZone;
                    var queryWorldPositionMsg = MessageFactory.GenerateQueryWorldPositionMsg();
                    queryWorldPositionMsg.DoAfter = position => _position = position;
                    _controller.gameObject.SendMessageTo(queryWorldPositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(queryWorldPositionMsg);

                    SubscribeToMessages();

                    ClientController.TransferPlayer(zone, DataController.CurrentCharacter.Position);
                }
                else
                {
                    var removateTraitFromUnitMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                    removateTraitFromUnitMsg.Controller = _controller;
                    _controller.gameObject.SendMessageTo(removateTraitFromUnitMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(removateTraitFromUnitMsg);
                }
            }
            else
            {
                var removateTraitFromUnitMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removateTraitFromUnitMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removateTraitFromUnitMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removateTraitFromUnitMsg);
            }
            
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryRecallMessage>(QueryRecall, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RecallMessage>(Recall, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveActiveRecallMessage>(RemoveActiveRecall, _instanceId);
        }

        private void QueryRecall(QueryRecallMessage msg)
        {
            msg.DoAfter.Invoke(_zone, _position);
        }

        private void Recall(RecallMessage msg)
        {
            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
            setUnitStateMsg.State = UnitState.Interaction;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            ClientController.TransferPlayer(_zone, _position);
            var removeTraitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitByControllerMsg);
        }

        private void RemoveActiveRecall(RemoveActiveRecallMessage msg)
        {
            var removeTraitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitByControllerMsg);
        }
    }
}