using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Secondary Stats Trait", menuName = "Ancible Tools/Traits/Combat/Apply Secondary Stats")]
    public class ApplySecondaryStatsTrait : Trait
    {
        [SerializeField] private SecondaryStats _stats;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applySecondaryStatsMsg = MessageFactory.GenerateApplySecondaryStatsMsg();
            applySecondaryStatsMsg.Stats = _stats;
            _controller.gameObject.SendMessageTo(applySecondaryStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applySecondaryStatsMsg);
        }

        public override void Destroy()
        {
            var applySecondaryStatsMsg = MessageFactory.GenerateApplySecondaryStatsMsg();
            applySecondaryStatsMsg.Stats = _stats * -1;
            _controller.gameObject.SendMessageTo(applySecondaryStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applySecondaryStatsMsg);
            base.Destroy();
        }
    }
}