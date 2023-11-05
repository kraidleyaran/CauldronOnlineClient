using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Interaction Trait", menuName = "Ancible Tools/Traits/Player/Player Interaction")]
    public class PlayerInteractionTrait : Trait
    {
        public const string ROLL = "Roll";

        [SerializeField] private int _rollCooldown = 30;

        private GameObject _interaction = null;
        private UnitState _unitState = UnitState.Active;

        private Sequence _rollCooldownSequence = null;
        private Vector2Int _direction = Vector2Int.zero;
        private bool _knockback = false;


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
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RollFinishedMessage>(RollFinished, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateKnockbackStateMessage>(UpdateKnockbackState, _instanceId);
        }

        private bool CanInteract()
        {
            return _unitState == UnitState.Active;
        }

        private bool CanRoll()
        {
            return _unitState == UnitState.Active && !_interaction && _direction != Vector2Int.zero && _rollCooldownSequence == null && !_knockback;
        }

        private void RollCooldownFinished()
        {
            _rollCooldownSequence = null;
            if (CanRoll())
            {
                UiOverlayWindow.SetActionText("Roll");
            }
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
                if (CanRoll())
                {
                    UiOverlayWindow.SetActionText("Roll");
                }
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.RightShoulder && msg.Current.RightShoulder)
            {
                if (_interaction)
                {
                    if (CanInteract())
                    {
                        _controller.gameObject.SendMessageTo(InteractMessage.INSTANCE, _interaction);
                    }
                }
                else if (CanRoll())
                {
                    _controller.gameObject.SendMessageTo(RollMessage.INSTANCE, _controller.transform.parent.gameObject);
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

        private void UpdateDirection(UpdateDirectionMessage msg)
        {
            _direction = msg.Direction;
            if (CanRoll())
            {
                UiOverlayWindow.SetActionText(ROLL);
            }
            else if (!_interaction)
            {
                UiOverlayWindow.ClearActionText();
            }
        }

        private void RollFinished(RollFinishedMessage msg)
        {
            _rollCooldownSequence = DOTween.Sequence().AppendInterval(_rollCooldown * TickController.TickRate).OnComplete(RollCooldownFinished);
        }

        private void UpdateKnockbackState(UpdateKnockbackStateMessage msg)
        {
            _knockback = msg.Active;
        }

        public override void Destroy()
        {
            _interaction = null;
            if (_rollCooldownSequence != null)
            {
                if (_rollCooldownSequence.IsActive())
                {
                    _rollCooldownSequence.Kill();
                }

                _rollCooldownSequence = null;
            }
            base.Destroy();
        }
    }
}