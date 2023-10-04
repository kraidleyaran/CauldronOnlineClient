using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Action World Item", menuName = "Ancible Tools/Items/Action Item")]
    public class ActionItem : WorldItem
    {
        public override ItemType Type => ItemType.Action;
        public WorldAbility Ability;
        public bool UseStack = false;

        public override string GetDescription()
        {
            var description = base.GetDescription();
            var abilityDescription = Ability.GetDescription();
            description = string.IsNullOrEmpty(description) ? abilityDescription : $"{description}{Environment.NewLine}{abilityDescription}";
            return description;
        }
    }
}