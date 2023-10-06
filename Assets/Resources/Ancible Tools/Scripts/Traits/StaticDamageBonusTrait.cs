using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Static Damage Bonus Trait", menuName = "Ancible Tools/Traits/Combat/Damage/Static Damage Bonus")]
    public class StaticDamageBonusTrait : Trait
    {
        [SerializeField] private int _amount;
        [SerializeField] private DamageType _type;
        [SerializeField] private bool _ignoreType = false;
        [SerializeField] private BonusTag[] _tags;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        public override BonusTag[] GetBonusTags()
        {
            return _tags;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDamageBonusMessage>(QueryDamageBonus, _instanceId);
        }

        private void QueryDamageBonus(QueryDamageBonusMessage msg)
        {
            if (_ignoreType || _type == msg.Type)
            {
                var apply = false;
                foreach (var tag in msg.Tags)
                {
                    if (_tags.Contains(tag))
                    {
                        apply = true;
                        break;
                    }
                }
                if (apply)
                {
                    msg.DoAfter.Invoke(_amount);
                }
            }
        }
    }
}