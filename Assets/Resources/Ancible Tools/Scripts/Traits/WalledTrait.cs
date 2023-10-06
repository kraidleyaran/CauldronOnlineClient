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

        private HitboxController _hitboxController = null;

        private ContactFilter2D _contactFilter = new ContactFilter2D();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.Casting);
            _contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = CollisionLayerFactory.Terrain.ToMask()
            };
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<WalledCheckMessage>(WalledCheck, _instanceId);
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
                    
                    var collision = results[i].point;
                    var collisionDirection = (collision - msg.Origin).ToDirection();
                    if (collisionDirection.x == intDirection.x || collisionDirection.y == intDirection.y)
                    {
                        
                        var distance = results[i].distance;
                        if (distance > 0f)
                        {
                            pos = msg.Origin + msg.Direction * (distance * results[i].fraction);
                        }
                        else
                        {
                            pos = msg.Origin;
                        }

                        collided = true;
                        break;
                    }
                }

            }
            msg.DoAfter.Invoke(pos, collided);
        }
    }
}