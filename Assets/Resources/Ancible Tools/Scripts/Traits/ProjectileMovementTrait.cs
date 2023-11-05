using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Projectile Movement Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Projectile Movement")]
    public class ProjectileMovementTrait : Trait
    {
        [SerializeField] private int _maxMoveEvents = 3;

        private Rigidbody2D _rigidBody = null;
        private Vector2 _direction = Vector2.zero;
        private int _speed = 0;
        private Trait[] _applyOnWallImpact = new Trait[0];
        private bool _reportMovement = false;
        private bool _stopOnWall = false;
        private bool _returning = false;
        private bool _unregister = false;
        private bool _applyingMovement = false;
        private int _networkDetectDistance = 4;
        private Vector2 _networkOwnerOffset = Vector2.zero;

        private GameObject _owner = null;
        private BonusTag[] _tags = new BonusTag[0];

        private List<MovementEvent> _movementEvent = new List<MovementEvent>();
        private WorldVector2Int _worldPosition = WorldVector2Int.Zero;
        private bool _networkReturn = false;
        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            _worldPosition = _rigidBody.position.ToWorldPosition();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupProjectileMessage>(SetupProjectile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryPositionMessage>(QueryPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetProjectileDirectionMessage>(SetProjectileDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetWorldPositionMessage>(SetWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateOwnerMessage>(UpdateOwner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryProjectileTagsMessage>(QueryProjectileTags, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<NetworkReturnToOwnerMessage>(NetworkReturnToOwner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<NetworkProjectileReturnedMessage>(NetworkProjectileReturned, _instanceId);
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            var moveSpeed = TickController.CalculateFixedMoveSpeed(_speed, true);
            if (_movementEvent.Count > 0)
            {
                var diff = (_movementEvent[0].Position.ToWorldVector() - _rigidBody.position);
                var distance = diff.magnitude;
                if (diff.normalized == Vector2.zero || distance <= moveSpeed)
                {
                    _rigidBody.position = _movementEvent[0].Position.ToWorldVector();
                    _worldPosition = _movementEvent[0].Position;
                    _movementEvent.RemoveAt(0);

                    var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                    updatePositionMsg.Position = _rigidBody.position;
                    _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updatePositionMsg);
                }
                else
                {
                    _rigidBody.position += diff.normalized * moveSpeed;

                    var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                    updatePositionMsg.Position = _rigidBody.position;
                    _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updatePositionMsg);
                }

                if (_movementEvent.Count > 0)
                {
                    _direction = (_movementEvent[0].Position.ToWorldVector() - _worldPosition.ToWorldVector()).normalized;
                }
            }
            else if (_networkReturn)
            {
                var diff = (_owner.transform.position.ToVector2() + _networkOwnerOffset) - _rigidBody.position;
                var distance = diff.magnitude;
                if (distance > _networkDetectDistance * DataController.Interpolation)
                {
                    var speed = Mathf.Max(TickController.WorldTick, TickController.CalculateFixedMoveSpeed(_speed) - TickController.TickRate);
                    _rigidBody.position += diff.normalized * speed;
                }
                else
                {
                    var projectileReturnedMsg = MessageFactory.GenerateProjectileReturnedMsg();
                    projectileReturnedMsg.Projectile = _controller.transform.parent.gameObject;
                    projectileReturnedMsg.Destroy = true;
                    _controller.gameObject.SendMessageTo(projectileReturnedMsg, _owner);
                    MessageFactory.CacheMessage(projectileReturnedMsg);
                }
            }
            else if (_direction != Vector2Int.zero && _speed > 0)
            {
                var setPos = _rigidBody.position;
                var wallChecked = false;
                var stop = false;
                if (!_returning)
                {
                    var walledCheckMsg = MessageFactory.GenerateWalledCheckMsg();

                    walledCheckMsg.Direction = _direction;
                    walledCheckMsg.Speed = moveSpeed;
                    walledCheckMsg.Origin = _rigidBody.position;
                    walledCheckMsg.CheckAlternate = false;
                    walledCheckMsg.DoAfter = (pos, collided) =>
                    {
                        setPos = pos;
                        stop = collided;
                        wallChecked = true;
                    };
                    _controller.gameObject.SendMessageTo(walledCheckMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(walledCheckMsg);
                }

                if (!wallChecked)
                {
                    setPos = _rigidBody.position + Vector2.ClampMagnitude(_direction * moveSpeed, moveSpeed);
                }
                if (stop)
                {
                    if (_movementEvent.Count > 0)
                    {
                        _movementEvent.RemoveAt(0);
                    }
                    if (_applyOnWallImpact.Length > 0)
                    {
                        var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                        foreach (var trait in _applyOnWallImpact)
                        {
                            addTraitToUnitMsg.Trait = trait;
                            _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                        }
                        MessageFactory.CacheMessage(addTraitToUnitMsg);
                        //var id = ObjectManager.GetId(_controller.transform.parent.gameObject);
                        //if (!string.IsNullOrEmpty(id))
                        //{
                        //    ClientController.SendToServer(new ClientDestroyObjectMessage { TargetId = id, Tick = TickController.ServerTick });
                        //}
                        //ObjectManager.DestroyNetworkObject(_controller.transform.parent.gameObject);
                    }
                    else if (_stopOnWall)
                    {
                        _direction = Vector2.zero;
                    }

                }
                else
                {
                    if (_movementEvent.Count > 0)
                    {
                        var distance = (_movementEvent[0].Position.ToWorldVector() - setPos).magnitude;
                        if (distance <= moveSpeed)
                        {
                            setPos = _movementEvent[0].Position.ToWorldVector();
                            _movementEvent.RemoveAt(0);
                        }
                        _rigidBody.position = setPos;
                        var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                        updatePositionMsg.Position = _rigidBody.position;
                        _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(updatePositionMsg);
                    }
                    else
                    {
                        _rigidBody.position = setPos;
                        var updatePositionMsg = MessageFactory.GenerateUpdatePositionMsg();
                        updatePositionMsg.Position = _rigidBody.position;
                        _controller.gameObject.SendMessageTo(updatePositionMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(updatePositionMsg);
                    }

                }

            }
            else if (_speed == 0 && _movementEvent.Count > 0)
            {
                _movementEvent.Clear();
            }


        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            var worldPosition = _rigidBody.position.ToWorldPosition();
            if (worldPosition != _worldPosition)
            {
                _worldPosition = worldPosition;
                ClientController.SendToServer(new ClientProjectileMovementUpdateMessage{TargetId = _worldId, Position = _worldPosition, Speed = _speed, Tick = TickController.ServerTick});
            }
        }

        private void SetupProjectile(SetupProjectileMessage msg)
        {
            _direction = msg.Direction;
            _speed = msg.MoveSpeed;
            _applyOnWallImpact = msg.ApplyOnWall;
            _stopOnWall = msg.StopOnWall;
            _reportMovement = msg.ReportPosition;
            _worldId = msg.WorldId;
            _unregister = msg.Unregister;
            _tags = msg.Tags.ToArray();
            if (_reportMovement)
            {
                _controller.gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
                _controller.transform.parent.gameObject.SubscribeWithFilter<ReturningToOwnerMessage>(ReturningToOwner, _instanceId);
            }
            else
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyMovementEventMessage>(ApplyMovementEvent, _instanceId);
            }
        }

        private void QueryPosition(QueryPositionMessage msg)
        {
            msg.DoAfter.Invoke(_rigidBody.position);
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            _direction = msg.Direction;
            var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
            updateDirectionMsg.Direction = msg.Direction;
            _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateDirectionMsg);
            //_movementEvent.Clear();
        }

        private void ApplyMovementEvent(ApplyMovementEventMessage msg)
        {
            _applyingMovement = true;
            while (_movementEvent.Count > _maxMoveEvents)
            {
                _movementEvent.RemoveAt(0);
            }
            _movementEvent.Add(msg.Event);
        }

        private void SetProjectileDirection(SetProjectileDirectionMessage msg)
        {
            _direction = msg.Direction;
            //_movementEvent.Clear();
        }

        private void SetWorldPosition(SetWorldPositionMessage msg)
        {
            _worldPosition = msg.Position;
            _rigidBody.position = msg.Position.ToWorldVector();
        }

        private void ReturningToOwner(ReturningToOwnerMessage msg)
        {
            _returning = true;
            _movementEvent.Clear();
        }

        private void UpdateOwner(UpdateOwnerMessage msg)
        {
            _owner = msg.Owner;
        }

        private void QueryProjectileTags(QueryProjectileTagsMessage msg)
        {
            foreach (var tag in msg.Tags)
            {
                if (_tags.Contains(tag))
                {
                    msg.DoAfter.Invoke();
                    break;
                }
            }
        }

        private void NetworkReturnToOwner(NetworkReturnToOwnerMessage msg)
        {
            _networkReturn = true;
            _networkDetectDistance = msg.DetectInstance;
            _networkOwnerOffset = msg.Offset.ToWorldVector();
            //var projectileReturnedMsg = MessageFactory.GenerateProjectileReturnedMsg();
            //projectileReturnedMsg.Projectile = _controller.transform.parent.gameObject;
            //projectileReturnedMsg.Destroy = true;
            //_controller.gameObject.SendMessageTo(projectileReturnedMsg, _owner);
            //MessageFactory.CacheMessage(projectileReturnedMsg);
        }

        private void NetworkProjectileReturned(NetworkProjectileReturnedMessage msg)
        {
            var projectileReturnedMsg = MessageFactory.GenerateProjectileReturnedMsg();
            projectileReturnedMsg.Projectile = _controller.transform.parent.gameObject;
            projectileReturnedMsg.Destroy = true;
            _controller.gameObject.SendMessageTo(projectileReturnedMsg, _owner);
            MessageFactory.CacheMessage(projectileReturnedMsg);
        }

        public override void Destroy()
        {
            if (_unregister && _owner)
            {
                var projectileReturnedMsg = MessageFactory.GenerateProjectileReturnedMsg();
                projectileReturnedMsg.Projectile = _controller.transform.parent.gameObject;
                projectileReturnedMsg.Destroy = false;
                _controller.gameObject.SendMessageTo(projectileReturnedMsg, _owner);
                MessageFactory.CacheMessage(projectileReturnedMsg);
            }
            base.Destroy();
        }
    }
}