using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Projectile Redirect Trait", menuName = "Ancible Tools/Traits/Interactable/Projectile Redirect")]
    public class ProjectileRedirectTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox;
        
        private Vector2Int _direction = Vector2Int.zero;
        private BonusTag[] _tags = new BonusTag[0];
        private HitboxController _hitboxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.MonsterHurt);
            _hitboxController.AddSubscriber(_controller.gameObject);
            SubscribeToMessages();
        }


        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupProjectileRedirectMessage>(SetupProjectileRedirect, _instanceId);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            var redirect = _tags.Length <= 0;
            if (!redirect)
            {

                var queryProjectileTagsMsg = MessageFactory.GenerateQueryProjectileTagsMsg();
                queryProjectileTagsMsg.DoAfter = () => redirect = true;
                queryProjectileTagsMsg.Tags = _tags;
                _controller.gameObject.SendMessageTo(queryProjectileTagsMsg, msg.Object);
                MessageFactory.CacheMessage(queryProjectileTagsMsg);
            }

            if (redirect)
            {
                _controller.gameObject.SendMessageTo(ResetMaxDistanceCheckMessage.INSTANCE, msg.Object);
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(setDirectionMsg, msg.Object);
                MessageFactory.CacheMessage(setDirectionMsg);
            }
        }

        private void SetupProjectileRedirect(SetupProjectileRedirectMessage msg)
        {
            _direction = msg.Direction.ToVector();
            _tags = ItemFactory.GetTags(msg.Tags);
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToWorldVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }
    }
}