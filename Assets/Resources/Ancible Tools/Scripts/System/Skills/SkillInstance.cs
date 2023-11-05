using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [Serializable]
    public class SkillInstance
    {
        public WorldSkill Skill;
        public int Experience;
        public int Level;

        public SkillInstance(WorldSkill skill)
        {
            Skill = skill;
        }

        public void SetFromData(GameObject owner, int experience, int level)
        {
            Experience = experience;
            Level = level;
            var max = Math.Min(Level, Skill.MaxLevel);
            for (var i = 0; i < max; i++)
            {
                Skill.ApplyLevel(owner,i + 1);
            }

            var requiredExperience = Skill.GetRequiredExperience(Level + 1);
            while (requiredExperience <= Experience)
            {
                Experience -= requiredExperience;
                Level++;
                Skill.ApplyLevel(owner, Level);
                requiredExperience = Skill.GetRequiredExperience(Level + 1);
            }
        }

        public bool GainExperience(GameObject owner, int experience)
        {
            Experience += experience;
            Debug.Log($"+{experience}{Skill.DisplayName}");
            var requiredExperience = Skill.GetRequiredExperience(Level + 1);
            var leveled = false;
            while (requiredExperience <= Experience)
            {
                Experience -= requiredExperience;
                Level++;
                Skill.ApplyLevel(owner, Level);
                leveled = true;
                requiredExperience = Skill.GetRequiredExperience(Level + 1);
            }

            return leveled;
        }

        public SkillData GetData()
        {
            return new SkillData {Name = Skill.name, Level = Level, Experience = Experience};
        }
    }
}