using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Aspects
{
    [CreateAssetMenu(fileName = "World Aspect", menuName = "Ancible Tools/World Aspect")]
    public class WorldAspect : ScriptableObject
    {
        public string DisplayName;
        public Sprite Icon;
        [TextArea(3, 5)] public string Description;
        public int MaxRanks;
        public WorldAspectRank[] Ranks;

        public void ApplyRank(int rank, GameObject owner)
        {
            var applyRank = Ranks.Where(r => r.MinimumLevel <= rank).OrderByDescending(r => r.MinimumLevel).FirstOrDefault() ?? Ranks[0];
            applyRank.Apply(owner);
        }

        public string GetDescription(int rank)
        {
            var description = Description;
            if (!string.IsNullOrEmpty(description))
            {
                description = $"{description}{Environment.NewLine}{Environment.NewLine}";
            }
            var applyRank = Ranks.Where(r => r.MinimumLevel <= rank).OrderByDescending(r => r.MinimumLevel).FirstOrDefault() ?? Ranks[0];
            if (applyRank != null)
            {
                var traits = applyRank.ApplyOnLevel.Where(t => t).Select(t => t.GetDescription()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
                foreach (var trait in traits)
                {
                    description = string.IsNullOrEmpty(description) ? $"{trait}" : $"{description}{Environment.NewLine}{trait}";
                }
            }
            return description;
        }
    }
}