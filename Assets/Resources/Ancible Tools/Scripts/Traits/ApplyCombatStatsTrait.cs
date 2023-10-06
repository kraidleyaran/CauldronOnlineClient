using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Combat Stats Trait", menuName = "Ancible Tools/Traits/Combat/Apply Combat Stats")]
    public class ApplyCombatStatsTrait : Trait
    {
        public override bool Instant => true;
        public override bool ApplyOnClient => false;

        [SerializeField] private CombatStats _stats;
        [SerializeField] private bool _bonus = true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applyCombatStatsMsg = MessageFactory.GenerateApplyCombatStatsMsg();
            applyCombatStatsMsg.Stats = _stats;
            applyCombatStatsMsg.Bonus = _bonus;
            _controller.gameObject.SendMessageTo(applyCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyCombatStatsMsg);
        }

        public override string GetDescription()
        {
            return _stats.ToDescriptionString();
        }
    }
}