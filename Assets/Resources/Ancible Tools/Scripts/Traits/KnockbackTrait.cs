using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Knockback Trait", menuName = "Ancible Tools/Traits/Combat/Knockback/Knockback")]
    public class KnockbackTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _speed = 1;
        [SerializeField] private int _distance = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            GameObject owner = null;

            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = (knockbackOwner) => { owner = knockbackOwner; };
            _controller.gameObject.SendMessageTo(queryOwnerMsg, controller.Sender);
            MessageFactory.CacheMessage(queryOwnerMsg);

            if (owner)
            {
                var id = owner == ObjectManager.Player ? ObjectManager.PlayerObjectId : ObjectManager.GetId(owner);
                var targetId = _controller.transform.parent.gameObject == ObjectManager.Player ? ObjectManager.PlayerObjectId : ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(id) && (targetId == ObjectManager.PlayerObjectId || id == ObjectManager.PlayerObjectId))
                {
                    if (!(controller.Sender is GameObject origin))
                    {
                        origin = owner;
                    }
                    var direction = (_controller.transform.parent.position.ToVector2() - origin.transform.position.ToVector2()).ToKnockbackDirection();
                    var applyKnockbackMsg = MessageFactory.GenerateApplyKnockbackMsg();
                    applyKnockbackMsg.Direction = direction;
                    applyKnockbackMsg.Distance = _distance;
                    applyKnockbackMsg.Speed = _speed;
                    applyKnockbackMsg.OwnerId = id;
                    _controller.gameObject.SendMessageTo(applyKnockbackMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(applyKnockbackMsg);
                }
            }
        }
    }
}