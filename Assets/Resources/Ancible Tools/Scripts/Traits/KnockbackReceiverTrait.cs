using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Knockback Receiver Trait", menuName = "Ancible Tools/Traits/Combat/Knockback/Knockback Receiver")]
    public class KnockbackReceiverTrait : Trait
    {
        [SerializeField] private bool _recieveKnockback = false;

        private Tween _knockbackTween = null;

        private Rigidbody2D _rigidBody = null;

        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void KnockbackComplete()
        {
            _knockbackTween = null;
            var updateKnockbackStateMsg = MessageFactory.GenerateUpdateKnockbackStateMsg();
            updateKnockbackStateMsg.Active = false;
            _controller.gameObject.SendMessageTo(updateKnockbackStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateKnockbackStateMsg);
        }

        private int CalculateKnockbackTicks(int speed, float distance)
        {
            return Mathf.RoundToInt(distance / (speed * TickController.WorldTick));
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyKnockbackEventMessage>(ApplyKnockbackEvent, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupKnockbackMessage>(SetupKnockback, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            if (_recieveKnockback)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyKnockbackMessage>(ApplyKnockback, _instanceId);
                _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDeathMessage>(UnitDeath, _instanceId);
            }
        }

        private void ApplyKnockback(ApplyKnockbackMessage msg)
        {
            if (_unitState != UnitState.Dead)
            {
                if (_knockbackTween != null)
                {
                    if (_knockbackTween.IsActive())
                    {
                        _knockbackTween.Kill();
                    }

                    _knockbackTween = null;
                }

                //var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                //setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                //_controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                //MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                var pos = _rigidBody.position;
                var wallChecked = false;
                var walledCheckMsg = MessageFactory.GenerateWalledCheckMsg();
                var speed = TickController.CalculateFixedMoveSpeed(msg.Speed);
                var distance = DataController.Interpolation * msg.Distance;
                walledCheckMsg.Direction = msg.Direction;
                walledCheckMsg.Speed = distance;
                walledCheckMsg.Origin = pos;
                walledCheckMsg.DoAfter = (setPos, contact) =>
                {
                    pos = setPos;
                    wallChecked = true;
                };
                _controller.gameObject.SendMessageTo(walledCheckMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(walledCheckMsg);
                if (wallChecked)
                {
                    distance = (pos - _rigidBody.position).magnitude;
                }
                else
                {
                    pos = _rigidBody.position + Vector2.ClampMagnitude(msg.Direction.ToVector2(false) * distance, distance);
                }

                var updateKnockbackStateMsg = MessageFactory.GenerateUpdateKnockbackStateMsg();
                updateKnockbackStateMsg.Active = true;
                _controller.gameObject.SendMessageTo(updateKnockbackStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateKnockbackStateMsg);

                var time = CalculateKnockbackTicks(msg.Speed, distance);

                _knockbackTween = _rigidBody.DOMove(pos, time * TickController.WorldTick).SetEase(Ease.Linear).OnComplete(KnockbackComplete);

                var targetId = _controller.transform.parent.gameObject == ObjectManager.Player ? ObjectManager.PlayerObjectId : ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(targetId))
                {
                    var worldPos = pos.ToWorldPosition();
                    ClientController.SendToServer(new ClientKnockbackMessage { OwnerId = msg.OwnerId, TargetId = targetId, Time = time, EndPosition = worldPos, Tick = TickController.ServerTick });
                }
            }
            
        }

        private void ApplyKnockbackEvent(ApplyKnockbackEventMessage msg)
        {
            if (_unitState != UnitState.Dead)
            {
                if (_knockbackTween != null)
                {
                    if (_knockbackTween.IsActive())
                    {
                        _knockbackTween.Kill();
                    }

                    _knockbackTween = null;
                }

                //var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                //setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                //_controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                //MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                var updateKnockbackStateMsg = MessageFactory.GenerateUpdateKnockbackStateMsg();
                updateKnockbackStateMsg.Active = true;
                _controller.gameObject.SendMessageTo(updateKnockbackStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateKnockbackStateMsg);

                var pos = msg.Position.ToWorldVector();
                _knockbackTween = _rigidBody.DOMove(pos, msg.Time * TickController.WorldTick).SetEase(Ease.Linear).OnComplete(KnockbackComplete);
            }
            
        }

        private void SetupKnockback(SetupKnockbackMessage msg)
        {
            if (!_recieveKnockback && msg.ReceiveKnockback)
            {
                _recieveKnockback = true;
                _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyKnockbackMessage>(ApplyKnockback, _instanceId);
                _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDeathMessage>(UnitDeath, _instanceId);
            }
        }

        private void UnitDeath(UnitDeathMessage msg)
        {
            if (_knockbackTween != null)
            {
                if (_knockbackTween.IsActive())
                {
                    _knockbackTween.Kill();

                }

                _knockbackTween = null;

                var updateKnockbackmsg = MessageFactory.GenerateUpdateKnockbackStateMsg();
                updateKnockbackmsg.Active = false;
                _controller.gameObject.SendMessageTo(updateKnockbackmsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateKnockbackmsg);
            }
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        public override void Destroy()
        {
            if (_knockbackTween != null)
            {
                if (_knockbackTween.IsActive())
                {
                    _knockbackTween.Kill();
                }

                _knockbackTween = null;
            }
            base.Destroy();
        }
    }
}