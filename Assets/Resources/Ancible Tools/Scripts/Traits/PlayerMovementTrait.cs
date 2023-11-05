using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Movement Trait", menuName = "Ancible Tools/Traits/Player/Player Movement")]
    public class PlayerMovementTrait : Trait
    {
        [SerializeField] private int _moveSpeed = 1;

        private Vector2Int _direction = Vector2Int.zero;
        private Rigidbody2D _rigidBody = null;

        private WorldVector2Int _worldPosition = WorldVector2Int.Zero;

        private bool _knockback = false;
        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = controller.transform.parent.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private bool CanMove(UnitState state)
        {
            return !_knockback && state != UnitState.Attack && state != UnitState.Dead && state != UnitState.Disabled && state != UnitState.Interaction && state != UnitState.Rolling;
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
            _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateKnockbackStateMessage>(UpdateKnockbackState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetWorldPositionMessage>(SetWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryWorldPositionMessage>(QueryWorldPosition, _instanceId);
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_direction != Vector2Int.zero && CanMove(_unitState))
            {
                var moveSpeed = TickController.CalculateFixedMoveSpeed(_moveSpeed, true);
                var wallContact = false;
                var walledCheckmsg = MessageFactory.GenerateWalledCheckMsg();
                walledCheckmsg.Direction = _direction;
                walledCheckmsg.Origin = _rigidBody.position;
                walledCheckmsg.Speed = moveSpeed;
                walledCheckmsg.CheckAlternate = true;
                var setPos = _rigidBody.position;
                walledCheckmsg.DoAfter = (pos, contact) =>
                {
                    setPos = pos;
                    wallContact = contact;
                };
                _controller.gameObject.SendMessageTo(walledCheckmsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(walledCheckmsg);

                if (!wallContact)
                {
                    _rigidBody.position = setPos;
                }
                //var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                //updatePositionMsg.Position = _rigidBody.position;
                //_controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                //MessageFactory.CacheMessage(updatePositionMsg);
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_unitState != UnitState.Dead)
            {
                var worldPosition = _rigidBody.position.ToWorldPosition();
                if (worldPosition != _worldPosition)
                {
                    _worldPosition = worldPosition;

                    var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
                    updateWorldPositionMsg.Position = _worldPosition;
                    _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateWorldPositionMsg);

                    if (CanMove(_unitState))
                    {
                        ClientController.SendToServer(new ClientMovementUpdateMessage { Position = _worldPosition, Speed = _moveSpeed, Tick = TickController.ServerTick });
                    }
                    var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                    updatePositionMsg.Position = _rigidBody.position;
                    _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updatePositionMsg);
                }
            }

            
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            if (_direction != msg.Direction && !_knockback && CanMove(_unitState))
            {
                _direction = msg.Direction;

                var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                updateDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateDirectionMsg);

                if (_direction == Vector2Int.zero)
                {
                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                    
                    var worldPosition = _worldPosition.ToWorldVector();
                    var diff = (worldPosition - _rigidBody.position);
                    var wallContact = false;
                    var walledCheckmsg = MessageFactory.GenerateWalledCheckMsg();
                    walledCheckmsg.Direction = diff.normalized;
                    walledCheckmsg.Origin = _rigidBody.position;
                    walledCheckmsg.Speed = diff.magnitude;
                    walledCheckmsg.CheckAlternate = false;
                    walledCheckmsg.DoAfter = (pos, contact) => wallContact = contact;
                    _controller.gameObject.SendMessageTo(walledCheckmsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(walledCheckmsg);

                    if (!wallContact)
                    {
                        _rigidBody.position = worldPosition;
                    }

                    var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                    updatePositionMsg.Position = _rigidBody.position;
                    _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updatePositionMsg);
                }
                else if (_direction != Vector2Int.zero)
                {
                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Move;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                }
            }
        }

        private void UpdateKnockbackState(UpdateKnockbackStateMessage msg)
        {
            _knockback = msg.Active;
            if (_knockback)
            {
                _direction = Vector2Int.zero;

                var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                updateDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateDirectionMsg);
            }
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState != UnitState.Active)
            {
                _direction = Vector2Int.zero;

                var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                updateDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateDirectionMsg);
            }
            else if (_direction != Vector2Int.zero)
            {
                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Move;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);
            }
        }

        private void SetWorldPosition(SetWorldPositionMessage msg)
        {
            _worldPosition = msg.Position;
            if (!msg.IgnorePositionChange)
            {
                _rigidBody.position = _worldPosition.ToWorldVector();
            }

            var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
            updateWorldPositionMsg.Position = _worldPosition;
            _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWorldPositionMsg);

            var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
            updatePositionMsg.Position = _rigidBody.position;
            _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updatePositionMsg);
        }

        private void QueryWorldPosition(QueryWorldPositionMessage msg)
        {
            msg.DoAfter.Invoke(_worldPosition);
        }
    }
}