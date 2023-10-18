using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Interaction Trait", menuName = "Ancible Tools/Traits/Player/Player Interaction")]
    public class PlayerInteractionTrait : Trait
    {
        private GameObject _interaction = null;
        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);

            _controller.transform.parent.gameObject.SubscribeWithFilter<SetInteractionMessage>(SetInteraction, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveInteractionMessage>(RemoveInteraction, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
        }

        private bool CanInteract()
        {
            return _unitState == UnitState.Active;
        }

        private void SetInteraction(SetInteractionMessage msg)
        {
            _interaction = msg.Interaction;
            UiOverlayWindow.SetActionText(msg.Action);
        }

        private void RemoveInteraction(RemoveInteractionMessage msg)
        {
            if (_interaction && _interaction == msg.Interaction)
            {
                _interaction = null;
                UiOverlayWindow.ClearActionText();
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (CanInteract() && _interaction)
            {
                if (!msg.Previous.RightShoulder && msg.Current.RightShoulder)
                {
                    _controller.gameObject.SendMessageTo(InteractMessage.INSTANCE, _interaction);
                }
            }
            
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState == UnitState.Interaction)
            {
                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);
            }
        }
    }
}