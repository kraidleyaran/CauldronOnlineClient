using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Movable Helper Trait", menuName = "Ancible Tools/Traits/Player/Movable Helper")]
    public class MovableHelperTrait : Trait
    {
        [SerializeField] private Vector2Int _offset = Vector2Int.zero;

        private GameObject _movable = null;
        private string _movableId = string.Empty;
        private Rigidbody2D _movableRigidBody = null;
        private int _moveSpeed = 0;
        private MovableAxis _axis = MovableAxis.Vertical;
        private Vector2Int _movableDirection = Vector2Int.zero;
        private WorldVector2Int _movablePosition = WorldVector2Int.Zero;
        private WorldOffset _movableOffset = new WorldOffset();

        private Rigidbody2D _rigidBody = null;
        private Vector2Int _direction = Vector2Int.zero;
        private WorldVector2Int _position = WorldVector2Int.Zero;
        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMovableMessage>(SetMovable, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReleaseMovableMessage>(ReleaseMovable, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
        }

        private void SetMovable(SetMovableMessage msg)
        {
            Debug.Log("Block grabbed");
            var active = _movable;
            _movable = msg.Movable;
            
            _movableId = msg.Id;
            _movableRigidBody = _movable.GetComponent<Rigidbody2D>();
            _moveSpeed = msg.MoveSpeed;
            _movableOffset = msg.Offset;
            
            _axis = msg.Axis;
            _movableDirection = (_movableRigidBody.position - _rigidBody.position).normalized.ToDirection();
            if (_axis == MovableAxis.Horizontal)
            {
                _movableDirection.y = 0;
            }
            else
            {
                _movableDirection.x = 0;
            }
            var setFacingDirectionMsg = MessageFactory.GenerateSetFacingDirectionMsg();
            setFacingDirectionMsg.Direction = _movableDirection;
            _controller.gameObject.SendMessageTo(setFacingDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setFacingDirectionMsg);

            if (!active)
            {
                var setAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setAnimationStateMsg.State = UnitAnimationState.Grab;
                _controller.gameObject.SendMessageTo(setAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setAnimationStateMsg);

                _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
                _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            }

            var offset = WorldVector2Int.Zero;
            var movablePosition = _movableRigidBody.position.ToWorldPosition();
            if (msg.Axis == MovableAxis.Horizontal)
            {
                movablePosition.Y = _rigidBody.position.ToWorldPosition().Y;
                offset.X = _movableDirection.x > 0 ? _movableOffset.Right : _movableOffset.Left;
            }
            else
            {
                movablePosition.X = _rigidBody.position.ToWorldPosition().X;
                offset.Y = _movableDirection.y > 0 ? _movableOffset.Up : _movableOffset.Down;
            }

            _position = movablePosition + offset;
            _rigidBody.position = _position.ToWorldVector();
            _movablePosition = _movableRigidBody.position.ToWorldPosition();
            ClientController.SendToServer(new ClientMovableUpdateMessage{MovableId = _movableId, Position = _position, MovablePosition = _movablePosition, MoveSpeed = _moveSpeed, Type = MovableType.Grab});;
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (msg.Current.RightShoulder && _movable)
            {
                var direction = Vector2Int.zero;
                switch (_axis)
                {
                    case MovableAxis.Horizontal:
                        if (msg.Current.Left)
                        {
                            direction = Vector2Int.left;
                        }
                        else if (msg.Current.Right)
                        {
                            direction = Vector2Int.right;
                        }
                        break;
                    case MovableAxis.Vertical:
                        if (msg.Current.Down)
                        {
                            direction = Vector2Int.down;
                        }
                        else if (msg.Current.Up)
                        {
                            direction = Vector2Int.up;
                        }
                        break;
                }

                if (direction != _direction)
                {
                    if (direction == Vector2Int.zero)
                    {
                        var position = _rigidBody.position.ToWorldPosition();
                        var movablePosition = _movableRigidBody.position.ToWorldPosition();
                        if (position != _position || movablePosition != _movablePosition)
                        {
                            _position = position;
                            //var offset = _movableDirection.x != 0 ? new Vector2Int(_offset.x * _movableDirection.x, 0) : new Vector2Int(0, _movableDirection.y * _offset.y);
                            //_rigidBody.position = _position.ToWorldVector();
                            _movablePosition = movablePosition;
                            //_movableRigidBody.position = _movablePosition.ToWorldVector();

                            var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                            setWorldPositionMsg.IgnorePositionChange = true;
                            setWorldPositionMsg.Position = _position;
                            _controller.gameObject.SendMessageTo(setWorldPositionMsg, _controller.transform.parent.gameObject);
                            setWorldPositionMsg.Position = _movablePosition;
                            _controller.gameObject.SendMessageTo(setWorldPositionMsg, _movable);
                            MessageFactory.CacheMessage(setWorldPositionMsg);

                            ClientController.SendToServer(new ClientMovableUpdateMessage { MoveSpeed = _moveSpeed, MovableId = _movableId, MovablePosition = _movablePosition, Position = _position, Tick = TickController.ServerTick, Type = MovableType.Grab });
                        }
                    }
                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = direction == Vector2Int.zero ? UnitAnimationState.Grab : UnitAnimationState.Push;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                }
                _direction = direction;
            }
            else
            {
                Debug.Log("Block Released");
                _controller.gameObject.SendMessageTo(ReleaseMovableMessage.INSTANCE, _movable);
                _controller.gameObject.SendMessageTo(ReleaseMovableMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_direction != Vector2Int.zero)
            {
                var moveSpeed = TickController.CalculateFixedMoveSpeed(_moveSpeed, true);
                var walledCheckmsg = MessageFactory.GenerateWalledCheckMsg();
                walledCheckmsg.Direction = _direction;
                walledCheckmsg.Origin = _movableRigidBody.position;
                walledCheckmsg.Speed = moveSpeed;
                walledCheckmsg.CheckAlternate = true;
                var setPos = _rigidBody.position;
                var wallContact = false;
                walledCheckmsg.DoAfter = (pos, contact) =>
                {
                    setPos = pos;
                    wallContact = contact;
                };
                //if (_movableDirection == _direction)
                //{
                //    walledCheckmsg.Ignore = new[] {_controller.transform.parent.gameObject};
                //}
                _controller.gameObject.SendMessageTo(walledCheckmsg, _movable);
                if (!wallContact && _movableDirection != _direction)
                {
                    walledCheckmsg.Origin = _rigidBody.position;
                    walledCheckmsg.DoAfter = (pos, contact) =>
                    {                        
                        wallContact = contact;
                    };
                    _controller.gameObject.SendMessageTo(walledCheckmsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(walledCheckmsg);

                if (!wallContact)
                {
                    var distance = (setPos - _movableRigidBody.position).magnitude;
                    _movableRigidBody.position = setPos;
                    _rigidBody.position += distance * _direction.ToVector2(false);
                }
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            var position = _rigidBody.position.ToWorldPosition();
            var movablePosition = _movableRigidBody.position.ToWorldPosition();
            if (position != _position || movablePosition != _movablePosition)
            {
                _position = position;
                _movablePosition = movablePosition;

                var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                setWorldPositionMsg.IgnorePositionChange = true;
                setWorldPositionMsg.Position = _position;
                _controller.gameObject.SendMessageTo(setWorldPositionMsg, _controller.transform.parent.gameObject);
                setWorldPositionMsg.Position = _movablePosition;
                _controller.gameObject.SendMessageTo(setWorldPositionMsg, _movable);
                MessageFactory.CacheMessage(setWorldPositionMsg);

                ClientController.SendToServer(new ClientMovableUpdateMessage{MoveSpeed = _moveSpeed, MovableId = _movableId, MovablePosition = _movablePosition, Position = _position, Tick = TickController.ServerTick, Type = MovableType.Move});
            }
        }

        private void ReleaseMovable(ReleaseMovableMessage msg)
        {
            if (_movable)
            {
                _movablePosition = _movableRigidBody.position.ToWorldPosition();
                //_position = _rigidBody.position.ToWorldPosition();
                ClientController.SendToServer(new ClientMovableUpdateMessage{MovableId = _movableId, MovablePosition = _movablePosition, Position = _position, MoveSpeed = _moveSpeed, Tick = TickController.ServerTick, Type = MovableType.Release});
                _movable = null;
                _movableId = string.Empty;
                _moveSpeed = 0;
                _movableRigidBody = null;

                _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();
                _controller.gameObject.Unsubscribe<UpdateTickMessage>();
                _controller.gameObject.Unsubscribe<UpdateInputStateMessage>();

                if (_unitState == UnitState.Interaction)
                {
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

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
        }
    }
}