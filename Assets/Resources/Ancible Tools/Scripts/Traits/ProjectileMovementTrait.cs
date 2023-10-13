using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Projectile Movement Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Projectile Movement")]
    public class ProjectileMovementTrait : Trait
    {
        private Rigidbody2D _rigidBody = null;
        private Vector2 _direction = Vector2.zero;
        private int _speed = 0;
        private Trait[] _applyOnWallImpact = new Trait[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupProjectileMessage>(SetupProjectile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryPositionMessage>(QueryPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_direction != Vector2Int.zero && _speed > 0)
            {
                var moveSpeed = TickController.CalculateFixedMoveSpeed(_speed, true);
                var walledCheckMsg = MessageFactory.GenerateWalledCheckMsg();
                walledCheckMsg.Direction = _direction;
                walledCheckMsg.Speed = moveSpeed;
                walledCheckMsg.Origin = _rigidBody.position;
                var setPos = _rigidBody.position;
                var stop = false;
                var wallChecked = false;
                walledCheckMsg.DoAfter = (pos, collided) =>
                {
                    setPos = pos;
                    stop = collided;
                    wallChecked = true;
                };
                _controller.gameObject.SendMessageTo(walledCheckMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(walledCheckMsg);
                if (!wallChecked)
                {
                    setPos = _rigidBody.position + Vector2.ClampMagnitude(_direction * moveSpeed, moveSpeed);
                }
                if (stop)
                {
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
                    else
                    {
                        _direction = Vector2.zero;
                    }

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

        private void SetupProjectile(SetupProjectileMessage msg)
        {
            _direction = msg.Direction;
            _speed = msg.MoveSpeed;
            _applyOnWallImpact = msg.ApplyOnWall;
        }

        private void QueryPosition(QueryPositionMessage msg)
        {
            msg.DoAfter.Invoke(_rigidBody.position);
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            _direction = msg.Direction;
        }
    }
}