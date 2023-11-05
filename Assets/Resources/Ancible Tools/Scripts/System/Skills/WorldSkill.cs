using System.Linq;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [CreateAssetMenu(fileName = "World Skill", menuName = "Ancible Tools/Word Skill")]
    public class WorldSkill : ScriptableObject
    {
        public string DisplayName;
        [TextArea(3, 5)] public string Description;
        public Sprite Icon;
        public SkillLevel[] Levels;
        public int MaxLevel;
        public int BaseExperience;
        public float Multiplier;

        public int GetRequiredExperience(int level)
        {
            if (level == 0)
            {
                return BaseExperience;
            }

            return BaseExperience + Mathf.RoundToInt(BaseExperience * (Multiplier * level));
        }

        public void ApplyLevel(GameObject owner, int level)
        {
            if (level + 1 <= MaxLevel)
            {
                var skillLevel = Levels.Where(l => l.RequiredLevel <= level + 1).OrderByDescending(l => l.RequiredLevel).FirstOrDefault();
                if (skillLevel != null)
                {
                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in skillLevel.ApplyOnLevel)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        owner.SendMessageTo(addTraitToUnitMsg, owner);
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);
                }
            }
        }
    }
}