using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Skill Manager Trait", menuName = "Ancible Tools/Traits/Skill/Skill Manager")]
    public class SkillManagerTrait : Trait
    {
        [SerializeField] private WorldSkill[] _startingSkills = new WorldSkill[0];

        private Dictionary<WorldSkill, SkillInstance> _skills = new Dictionary<WorldSkill, SkillInstance>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            foreach (var skill in _startingSkills)
            {
                if (!_skills.ContainsKey(skill))
                {
                    _skills.Add(skill, new SkillInstance(skill));
                }
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<GainSkillExperienceMessage>(GainSkillExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QuerySkillsMessage>(QuerySkills, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSkillsMessage>(SetSkills, _instanceId);
        }

        private void GainSkillExperience(GainSkillExperienceMessage msg)
        {
            if (!_skills.TryGetValue(msg.Skill, out var instance))
            {
                instance = new SkillInstance(msg.Skill);
                _skills.Add(msg.Skill, instance);
            }
            
            if (instance.GainExperience(_controller.transform.parent.gameObject, msg.Experience))
            {
                //TODO: Show skill level gain;
            }
            _controller.gameObject.SendMessage(SkillsUpdatedMessage.INSTANCE);
        }

        private void QuerySkills(QuerySkillsMessage msg)
        {
            msg.DoAfter.Invoke(_skills.Values.ToArray());
        }

        private void SetSkills(SetSkillsMessage msg)
        {
            foreach (var data in msg.Skills)
            {
                var skill = SkillFactory.GetSkillByName(data.Name);
                if (skill)
                {
                    if (!_skills.TryGetValue(skill, out var instance))
                    {
                        instance = new SkillInstance(skill);
                        _skills.Add(skill, instance);
                    }

                    instance.SetFromData(_controller.transform.parent.gameObject, data.Experience, data.Level);
                }
            }
        }
    }
}