using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    [CreateAssetMenu(fileName = "World Ability", menuName = "Ancible Tools/Abilities/World Ability")]
    public class WorldAbility : ScriptableObject
    {
        public string DisplayName;
        [TextArea(3, 5)] public string Description;
        public ResourceItemStack[] RequiredResources;
        public Sprite Icon;
        public AbilityStep[] AbilitySteps = new AbilityStep[0];
        public SpriteTrait WeaponSprite;
        public AttackSetup AttackSetup;
        public Vector2Int Offset;
        public int Cooldown = 0;
        public int ManaCost = 0;

        public string GetDescription()
        {
            var description = Description;
            var traitDescriptions = new List<string>();
            foreach (var step in AbilitySteps)
            {
                traitDescriptions.AddRange(step.ApplyToAttack.GetTraitDescriptions());
                traitDescriptions.AddRange(step.ApplyToOwner.GetTraitDescriptions());
            }

            foreach (var trait in traitDescriptions)
            {
                description = string.IsNullOrEmpty(description) ? trait : $"{description}{Environment.NewLine}{trait}";
            }

            if (ManaCost > 0)
            {
                var manaLine = $"Mana:{ManaCost}";
                description = string.IsNullOrEmpty(description) ? manaLine : $"{description}{Environment.NewLine}{manaLine}";
            }
            if (RequiredResources.Length > 0)
            {
                var requiredDesriptions = RequiredResources.GetRequiredStacks();
                var resourcesLine = $"Resources:{requiredDesriptions.ToCommaDelimitedLine()}";
                description = string.IsNullOrEmpty(description) ? resourcesLine : $"{description}{Environment.NewLine}{resourcesLine}";
            }
            var tags = GetAllBonusTags();
            if (tags.Length > 0)
            {
                var tagNames = tags.ToDisplayNames();
                var tagsLine = $"Tags:{tagNames.ToCommaDelimitedLine()}";
                description = string.IsNullOrEmpty(description) ? $"{tagsLine}" : $"{description}{Environment.NewLine}{tagsLine}";
            }

            return description;
        }

        public int GetWorldTicks(int framesPerWorldTick)
        {
            var frames = 0;
            foreach (var step in AbilitySteps)
            {
                frames += step.Frames;
            }

            var worldTicks = frames / framesPerWorldTick;
            var frameCheck = worldTicks * framesPerWorldTick;
            if (frames < frameCheck)
            {
                worldTicks++;
            }
            return worldTicks;
        }

        public int GetRequiredIdCount()
        {
            var count = 0;
            foreach (var step in AbilitySteps)
            {
                foreach (var trait in step.ApplyToAttack)
                {
                    if (trait.RequireId)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public BonusTag[] GetAllBonusTags()
        {
            var tags = new List<BonusTag>();
            foreach (var step in AbilitySteps)
            {
                var allTags = step.ApplyToAttack.Select(t => t.GetBonusTags()).ToList();
                allTags.AddRange(step.ApplyToOwner.Select(t => t.GetBonusTags()));
                foreach (var stepTagSet in allTags)
                {
                    var newTags = stepTagSet.Where(t => !tags.Contains(t)).ToArray();
                    if (newTags.Length > 0)
                    {
                        tags.AddRange(newTags);
                    }
                }
            }

            return tags.ToArray();
        }
    }
}