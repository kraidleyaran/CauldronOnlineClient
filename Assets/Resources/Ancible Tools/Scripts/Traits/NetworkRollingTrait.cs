using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Rolling Trait", menuName = "Ancible Tools/Traits/Network/Network Rolling")]
    public class NetworkRollingTrait : Trait
    {
        private UnitState _unitState = UnitState.Active;
        private Rigidbody2D _rigidBody = null;

        private List<RollEvent> _rollEvents = new List<RollEvent>();
        private Tween _correctingTween = null;
        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<RollEventMessage>(RollEvent, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateKnockbackStateMessage>(UpdateKnockbackState, _instanceId);
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_rollEvents.Count > 0)
            {
                var rollEvent = _rollEvents[0];
                var speed = TickController.CalculateFixedMoveSpeed(rollEvent.Speed);
                var diff = rollEvent.Position.ToWorldVector() - _rigidBody.position;
                var direction = diff.normalized;
                var addPos = Vector2.ClampMagnitude(direction * speed, speed);
                var newPos = _rigidBody.position + addPos;
                if (diff.magnitude > speed)
                {
                    _rigidBody.position = newPos;
                }
                else
                {
                    _rigidBody.position = rollEvent.Position.ToWorldVector();
                    if (rollEvent.Finished)
                    {
                        _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();
                        var setAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                        setAnimationStateMsg.State = UnitAnimationState.Idle;
                        _controller.gameObject.SendMessageTo(setAnimationStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setAnimationStateMsg);

                        var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                        setUnitStateMsg.State = UnitState.Active;
                        _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setUnitStateMsg);
                    }
                    _rollEvents.RemoveAt(0);
                }

            }
        }

        private void RollEvent(RollEventMessage msg)
        {
            _rollEvents.Add(msg.Event);
            if (_unitState != UnitState.Rolling)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Rolling;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                var setAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setAnimationStateMsg.State = UnitAnimationState.Roll;
                _controller.gameObject.SendMessageTo(setAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setAnimationStateMsg);

                var setFacingDirectionMsg = MessageFactory.GenerateSetFacingDirectionMsg();
                setFacingDirectionMsg.Direction = _rollEvents[0].Direction.ToVector();
                _controller.gameObject.SendMessageTo(setFacingDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setFacingDirectionMsg);

                _controller.gameObject.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
            }
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            if (_unitState == UnitState.Rolling && msg.State != UnitState.Rolling && _rollEvents.Count > 0)
            {
                _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();
                _rollEvents.Clear();
            }
        }

        private void UpdateKnockbackState(UpdateKnockbackStateMessage msg)
        {
            if (msg.Active)
            {
                _rollEvents.Clear();
                if (_unitState == UnitState.Rolling)
                {
                    _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();
                    var setAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setAnimationStateMsg.State = UnitAnimationState.Idle;
                    _controller.gameObject.SendMessageTo(setAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setAnimationStateMsg);

                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Active;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitStateMsg);
                }
            }

        }

        public override void Destroy()
        {
            _rollEvents.Clear();
            base.Destroy();
        }
    }
}