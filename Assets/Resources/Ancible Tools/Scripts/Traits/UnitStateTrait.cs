using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Unit State Trait", menuName = "Ancible Tools/Traits/General/Unit State")]
    public class UnitStateTrait : Trait
    {
        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitStateMessage>(SetUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryUnitStateMessage>(QueryUnitState, _instanceId);
        }

        private void SetUnitState(SetUnitStateMessage msg)
        {
            if (_unitState != msg.State)
            {
                _unitState = msg.State;
                var updateUnitStateMsg = MessageFactory.GenerateUpdateUnitStateMsg();
                updateUnitStateMsg.State = _unitState;
                _controller.gameObject.SendMessageTo(updateUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateUnitStateMsg);

                if (_unitState == UnitState.Disabled)
                {
                    _controller.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        private void QueryUnitState(QueryUnitStateMessage msg)
        {
            msg.DoAfter.Invoke(_unitState);
        }
    }
}