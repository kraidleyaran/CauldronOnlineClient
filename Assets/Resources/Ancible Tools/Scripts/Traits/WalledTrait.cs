using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Walled Trait", menuName = "Ancible Tools/Traits/General/Walled")]
    public class WalledTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox = null;
        [SerializeField] private int _resultCount = 5;
        [SerializeField] private bool _ignoreGround = false;
        [SerializeField] private bool _checkForPlayers = false;

        private HitboxController _hitboxController = null;

        private ContactFilter2D _contactFilter = new ContactFilter2D();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.Casting);
            var layerMask = CollisionLayerFactory.GroundTerrain.ToMask() | CollisionLayerFactory.Terrain.ToMask() | CollisionLayerFactory.InvisibleTerrain.ToMask() | CollisionLayerFactory.InvisibleGround.ToMask();
            if (_ignoreGround)
            {
                layerMask = CollisionLayerFactory.Terrain.ToMask() | CollisionLayerFactory.InvisibleTerrain.ToMask();
            }

            if (_checkForPlayers)
            {
                layerMask ^= CollisionLayerFactory.PlayerTerrain.ToMask();
            }

            _contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = layerMask
            };
            SubscribeToMessages();
        }

        private Vector2 GetAlternatePosition(Vector2Int direction, Vector2 origin, Vector2 centroid, float speed)
        {
            var collided = false;
            if (direction.x != 0)
            {
                var horizontalDirection = new Vector2Int(direction.x, 0);
                var horizontalResults = new RaycastHit2D[_resultCount];
                var xCount = _hitboxController.Collider2d.Cast(horizontalDirection, _contactFilter, horizontalResults, speed, true);
                if (xCount > 0)
                {
                    for (var i = 0; i < xCount; i++)
                    {
                        var collisionDirection = (horizontalResults[i].point - centroid).ToDirection();
                        if (collisionDirection.x == direction.x)
                        {
                            collided = true;
                            break;
                        }
                    }

                    if (!collided)
                    {
                        return origin + horizontalDirection.ToVector2(false) * speed;
                    }
                }
                else
                {
                    return origin + horizontalDirection.ToVector2(false) * speed;
                }
            }

            if (collided && direction.y != 0)
            {
                var verticalDirection = new Vector2Int(0, direction.y);
                var verticalResults = new RaycastHit2D[_resultCount];
                var yCount = _hitboxController.Collider2d.Cast(verticalDirection, _contactFilter, verticalResults, speed, true);
                if (yCount > 0)
                {
                    for (var i = 0; i < yCount; i++)
                    {
                        var collisionDirection = (verticalResults[i].point - centroid).ToDirection();
                        if (collisionDirection.y == direction.y)
                        {
                            return origin;
                        }
                    }

                    return origin + verticalDirection.ToVector2(false) * speed;
                }
                else
                {
                    return origin + verticalDirection.ToVector2(false) * speed;
                }
            }

            return origin;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<WalledCheckMessage>(WalledCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupWalledMessage>(SetupWalled, _instanceId);
        }

        private void WalledCheck(WalledCheckMessage msg)
        {
            var results = new RaycastHit2D[_resultCount];
            var count = _hitboxController.Collider2d.Cast(msg.Direction, _contactFilter, results, msg.Speed, true);
            var addPos = Vector2.ClampMagnitude(msg.Direction * msg.Speed, msg.Speed);
            var pos = msg.Origin + addPos;
            var collided = false;
            if (count > 0)
            {
                var intDirection = msg.Direction.ToDirection();
                for (var i = 0; i < count; i++)
                {
                    if (!msg.Ignore.Contains(results[i].rigidbody.gameObject))
                    {
                        var result = results[i];
                        var collision = results[i].point;
                        var collisionDirection = (collision - result.centroid).ToDirection();
                        if (collisionDirection == intDirection)
                        {
                            if (msg.CheckAlternate)
                            {
                                pos = GetAlternatePosition(intDirection, msg.Origin, result.centroid, msg.Speed / 2f);
                            }
                            if (!msg.CheckAlternate || pos == msg.Origin)
                            {
                                var distance = results[i].distance;
                                if (distance > DataController.Interpolation * 1)
                                {
                                    var setDistance = distance * results[i].fraction;
                                    if (_hitboxController.Collider2d.Cast(msg.Direction, _contactFilter, new List<RaycastHit2D>(5), setDistance) <= 0)
                                    {
                                        pos = msg.Origin + msg.Direction * setDistance;
                                    }
                                    else
                                    {
                                        pos = msg.Origin;
                                    }
                                }
                                else
                                {
                                    pos = msg.Origin;
                                }

                                collided = true;
                            }

                            break;
                        }

                        if (collisionDirection.x != 0 && collisionDirection.x == intDirection.x || collisionDirection.y != 0 && collisionDirection.y == intDirection.y)
                        {
                            if (msg.CheckAlternate)
                            {
                                pos = GetAlternatePosition(intDirection, msg.Origin, result.centroid, msg.Speed / 2f);
                            }
                            if (!msg.CheckAlternate || pos == msg.Origin)
                            {
                                pos = msg.Origin;
                                collided = true;
                            }
                            break;
                        }
                    }
                }

            }
            msg.DoAfter.Invoke(pos, collided);
        }

        private void SetupWalled(SetupWalledMessage msg)
        {
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
            _ignoreGround = msg.IgnoreGround;
            _checkForPlayers = msg.CheckForPlayer;

            var layerMask = CollisionLayerFactory.GroundTerrain.ToMask() | CollisionLayerFactory.Terrain.ToMask() | CollisionLayerFactory.InvisibleTerrain.ToMask() | CollisionLayerFactory.InvisibleGround.ToMask();
            if (_ignoreGround)
            {
                layerMask = CollisionLayerFactory.Terrain.ToMask() | CollisionLayerFactory.InvisibleTerrain.ToMask();
            }

            if (_checkForPlayers)
            {
                //This is incorrect - need to flip bit for the position in question
                //Maybe use the ToLayer function since it returns the int layer?
                layerMask ^= CollisionLayerFactory.PlayerTerrain.ToMask();
            }

            _contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = layerMask
            };
            Debug.Log($"{_contactFilter.layerMask}");
        }
    }
}