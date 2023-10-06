using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Trait", menuName = "Ancible Tools/Traits/Player/Player")]
    public class PlayerTrait : Trait
    {
        private UnitState _unitState = UnitState.Active;
        private bool _playerMenuOpen = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDeathMessage>(UnitDeath, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<CombatStatsUpdatedMessage>(CombatStatsUpdated, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
        }

        private void UnitDeath(UnitDeathMessage msg)
        {
            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
            setUnitStateMsg.State = UnitState.Dead;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            ClientController.RespawnPlayer();
        }

        private void CombatStatsUpdated(CombatStatsUpdatedMessage msg)
        {
            _controller.gameObject.SendMessage(PlayerCombatStatsUpdatedMessage.INSTANCE);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.PlayerMenu && msg.Current.PlayerMenu)
            {
                if (_unitState != UnitState.Dead && _unitState != UnitState.Disabled && _unitState != UnitState.Attack)
                {
                    if (_unitState == UnitState.Interaction)
                    {
                        if (_playerMenuOpen)
                        {
                            UiController.TogglePlayerMenu();

                            _playerMenuOpen = false;
                            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                            setUnitStateMsg.State = UnitState.Active;
                            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setUnitStateMsg);
                        }
                    }
                    else if (!_playerMenuOpen)
                    {
                        var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                        setUnitStateMsg.State = UnitState.Interaction;
                        _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setUnitStateMsg);

                        UiController.TogglePlayerMenu();

                        _playerMenuOpen = true;
                    }
                }
            }

        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
        }
    }
}