using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Percent Damage Bonus Trait", menuName = "Ancible Tools/Traits/Combat/Damage/Percent Damage Bonus")]
    public class PercentDamageBonusTrait : Trait
    {
        [SerializeField] private float _amount = 0f;
        [SerializeField] private DamageType _type = DamageType.Physical;
        [SerializeField] private bool _ignoreType = false;
        [SerializeField] private BonusTag[] _tags;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDamageBonusMessage>(QueryDamageBonus, _instanceId);
        }

        private void QueryDamageBonus(QueryDamageBonusMessage msg)
        {
            if (_ignoreType || msg.Type == _type)
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
                    msg.DoAfter.Invoke(Mathf.RoundToInt(msg.Amount * _amount));
                }
            }
        }
    }
}