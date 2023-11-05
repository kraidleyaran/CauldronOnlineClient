using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Movement Trait", menuName = "Ancible Tools/Traits/Network/Network Movement")]
    public class NetworkMovementTrait : Trait
    {
        [SerializeField] private int _maxMovementEvents = 5;

        private List<MovementEvent> _movementEvents = new List<MovementEvent>();

        private Rigidbody2D _rigidBody = null;

        private Vector2Int _direction = Vector2Int.zero;
        private WorldVector2Int _worldPosition = WorldVector2Int.Zero;
        private MovementEvent _currentEvent = null;
        private bool _knockback = false;

        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.GetComponent<Rigidbody2D>();
            _worldPosition = _rigidBody.position.ToWorldPosition();
            SubscribeToMessages();
        }

        private bool CanMove()
        {
            if (!_knockback)
            {
                return _unitState == UnitState.Active || _unitState == UnitState.Move;
            }

            return !_knockback;
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);

            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyMovementEventMessage>(ApplyMovement, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateKnockbackStateMessage>(UpdateKnockbackState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetPositionMessage>(SetPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetWorldPositionMessage>(SetWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetPathMessage>(SetPath, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryWorldPositionMessage>(QueryWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFacingDirectionMessage>(UpdateFacingDirection, _instanceId);
        }

        private void ApplyMovement(ApplyMovementEventMessage msg)
        {
            if (!_knockback)
            {
                _movementEvents.Add(msg.Event);
                if (_movementEvents.Count > _maxMovementEvents)
                {
                    if (_currentEvent != null)
                    {
                        _rigidBody.position = _currentEvent.Position.ToWorldVector();
                        _currentEvent = null;
                    }
                    while (_movementEvents.Count > _maxMovementEvents)
                    {
                        _movementEvents.RemoveAt(0);
                    }
                }
            }


        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            var canMove = CanMove();
            if (_currentEvent == null && _movementEvents.Count > 0 && canMove)
            {
                _currentEvent = _movementEvents[0];
                _movementEvents.RemoveAt(0);

                var direction = (_worldPosition).Direction(_currentEvent.Position).ToVector();
                if (direction != _direction)
                {
                    _direction = direction;
                    var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                    updateDirectionMsg.Direction = _direction;
                    _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateDirectionMsg);
                }

                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Move;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);
            }

            if (canMove)
            {
                if (_currentEvent != null)
                {
                    var moveSpeed = TickController.CalculateFixedMoveSpeed(_currentEvent.Speed);
                    var moveToPosition = _currentEvent.Position.ToWorldVector();
                    var diff = (moveToPosition - _rigidBody.position);
                    var direction = diff.normalized;

                    var addPos = Vector2.ClampMagnitude(direction * moveSpeed, moveSpeed);
                    var newPos = _rigidBody.position + addPos;
                    var distance = diff.magnitude;
                    if (distance > moveSpeed)
                    {
                        _rigidBody.position = newPos;
                    }
                    else
                    {
                        _rigidBody.position = moveToPosition;
                        _currentEvent = null;
                    }
                }
                else if (_movementEvents.Count <= 0)
                {
                    //_direction = Vector2Int.zero;

                    //var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                    //updateDirectionMsg.Direction = _direction;
                    //_controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                    //MessageFactory.CacheMessage(updateDirectionMsg);

                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                }
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            var worldPosition = _rigidBody.position.ToWorldPosition();
            if (worldPosition != _worldPosition)
            {
                _worldPosition = worldPosition;

                var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
                updateWorldPositionMsg.Position = _worldPosition;
                _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateWorldPositionMsg);
            }
        }

        private void UpdateKnockbackState(UpdateKnockbackStateMessage msg)
        {
            if (msg.Active)
            {
                _movementEvents.Clear();
                _currentEvent = null;
                _direction = Vector2Int.zero;
            }

            _knockback = msg.Active;
        }

        private void SetPosition(SetPositionMessage msg)
        {
            _rigidBody.position = msg.Position;
            _worldPosition = _rigidBody.position.ToWorldPosition();

            var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
            updateWorldPositionMsg.Position = _worldPosition;
            _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWorldPositionMsg);
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            if (_direction != msg.Direction)
            {
                _direction = msg.Direction;
                var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                updateDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateDirectionMsg);
            }
        }

        private void UpdateFacingDirection(UpdateFacingDirectionMessage msg)
        {
            _direction = msg.Direction;
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (CanMove())
            {
                if (_currentEvent != null || _movementEvents.Count > 0)
                {
                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Move;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                }
            }
            else
            {
                _currentEvent = null;
                _movementEvents.Clear();
            }
        }

        private void SetWorldPosition(SetWorldPositionMessage msg)
        {
            _rigidBody.position = msg.Position.ToWorldVector();
            _worldPosition = msg.Position;

            var updateWorldPositionMsg = MessageFactory.GenerateUpdateWorldPositionMsg();
            updateWorldPositionMsg.Position = _worldPosition;
            _controller.gameObject.SendMessageTo(updateWorldPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWorldPositionMsg);
        }

        private void SetPath(SetPathMessage msg)
        {
            _currentEvent = null;
            _movementEvents.Clear();
            foreach (var position in msg.Path)
            {
                _movementEvents.Add(new MovementEvent{Position = position, Speed = msg.MoveSpeed});
            }
        }

        private void QueryWorldPosition(QueryWorldPositionMessage msg)
        {
            msg.DoAfter.Invoke(_worldPosition);
        }
    }
}