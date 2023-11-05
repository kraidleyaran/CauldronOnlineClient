using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Rolling Trait", menuName = "Ancible Tools/Traits/Player/Rolling")]
    public class RollingTrait : Trait
    {
        [SerializeField] private int _rollSpeed = 0;
        [SerializeField] private int _rollDistance = 0;

        private Rigidbody2D _rigidBody = null;
        private float _currentDistance = 0f;
        private UnitState _state = UnitState.Active;
        private Vector2Int _direction = Vector2Int.zero;
        private WorldVector2Int _position = WorldVector2Int.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<RollMessage>(Roll, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWorldPositionMessage>(UpdateWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateKnockbackStateMessage>(UpdateKnockbackState, _instanceId);
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_state == UnitState.Rolling)
            {
                var maxDistance = _rollDistance * DataController.Interpolation;
                if (_currentDistance < maxDistance)
                {
                    var speed = TickController.CalculateFixedMoveSpeed(_rollSpeed, true);
                    var distance = speed;
                    if (_currentDistance + distance >= maxDistance)
                    {
                        distance = maxDistance - _currentDistance;
                    }

                    _currentDistance += distance;
                    var setPos = _rigidBody.position;
                    var walledCheckMsg = MessageFactory.GenerateWalledCheckMsg();
                    walledCheckMsg.Origin = _rigidBody.position;
                    walledCheckMsg.Direction = _direction;
                    walledCheckMsg.Speed = distance;
                    walledCheckMsg.CheckAlternate = true;
                    walledCheckMsg.DoAfter = (pos, contact) =>
                    {
                        setPos = pos;
                    };
                    _controller.gameObject.SendMessageTo(walledCheckMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(walledCheckMsg);

                    _rigidBody.position = setPos;
                }
                else
                {
                    _currentDistance = 0f;
                    _position = _rigidBody.position.ToWorldPosition();
                    _controller.gameObject.SendMessageTo(RollFinishedMessage.INSTANCE, _controller.transform.parent.gameObject);
                    _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();

                    var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                    setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                    _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Active;
                    _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setUnitStateMsg);

                    ClientController.SendToServer(new ClientRollUpdateMessage { Position = _position, MoveSpeed = _rollSpeed, Tick = TickController.ServerTick, Finished = true, Direction = _direction.ToWorldVector()});
                }
                
            }
            else
            {
                _position = _rigidBody.position.ToWorldPosition();
                _currentDistance = 0f;
                _controller.gameObject.SendMessageTo(RollFinishedMessage.INSTANCE, _controller.transform.parent.gameObject);
                _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();

                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Active;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                ClientController.SendToServer(new ClientRollUpdateMessage { Position = _position, MoveSpeed = _rollSpeed, Tick = TickController.ServerTick, Finished = true, Direction = _direction.ToWorldVector()});
            }
        }

        private void Roll(RollMessage msg)
        {
            if (_state != UnitState.Rolling)
            {
                _currentDistance = 0f;
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Rolling;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Roll;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
                ClientController.SendToServer(new ClientRollUpdateMessage { Position = _position, MoveSpeed = _rollSpeed, Tick = TickController.ServerTick, Finished = false, Direction = _direction.ToWorldVector() });
            }
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            if (_state == UnitState.Rolling && msg.State != UnitState.Rolling)
            {
                _currentDistance = 0f;
                _controller.gameObject.SendMessageTo(RollFinishedMessage.INSTANCE, _controller.transform.parent.gameObject);
                _controller.gameObject.Unsubscribe<FixedUpdateTickMessage>();

                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                ClientController.SendToServer(new ClientRollUpdateMessage { Position = _position, MoveSpeed = _rollSpeed, Tick = TickController.ServerTick, Finished = true, Direction = _direction.ToWorldVector()});
            }
            _state = msg.State;
        }

        private void UpdateDirection(UpdateDirectionMessage msg)
        {
            if (msg.Direction != Vector2Int.zero)
            {
                _direction = msg.Direction;
            }
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            _position = msg.Position;
            if (_state == UnitState.Rolling)
            {
                ClientController.SendToServer(new ClientRollUpdateMessage { Position = _position, MoveSpeed = _rollSpeed, Tick = TickController.ServerTick, Finished = false, Direction = _direction.ToWorldVector()});
            }
        }

        private void UpdateKnockbackState(UpdateKnockbackStateMessage msg)
        {
            if (msg.Active)
            {
                if (_state == UnitState.Rolling)
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
    }
}