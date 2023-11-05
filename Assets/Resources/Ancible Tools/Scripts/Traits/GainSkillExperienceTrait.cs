using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Gain Skill Experience Trait", menuName = "Ancible Tools/Traits/Skill/Gain Skill Experience")]
    public class GainSkillExperienceTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private WorldSkill _skill;
        [SerializeField] private int _experience = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            var gainSkillExperienceMsg = MessageFactory.GenerateGainSkillExperienceMsg();
            gainSkillExperienceMsg.Experience = _experience;
            gainSkillExperienceMsg.Skill = _skill;
            _controller.gameObject.SendMessageTo(gainSkillExperienceMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(gainSkillExperienceMsg);
        }
    }
}