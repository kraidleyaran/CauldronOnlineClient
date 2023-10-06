using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Armor World Item", menuName = "Ancible Tools/Items/Armor Item")]
    public class ArmorItem : WorldItem
    {
        public override ItemType Type => ItemType.Armor;
        public ArmorSlot Slot;

        [SerializeField] private Trait[] _applyOnEquip;

        public TraitController[] Equip(GameObject owner)
        {
            var returnControllers = new List<TraitController>();
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            foreach (var trait in _applyOnEquip)
            {
                addTraitToUnitMsg.Trait = trait;
                if (trait.Instant)
                {
                    addTraitToUnitMsg.DoAfter = null;
                }
                else
                {
                    addTraitToUnitMsg.DoAfter = controller => returnControllers.Add(controller);
                }
                owner.SendMessageTo(addTraitToUnitMsg, owner);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);

            return returnControllers.ToArray();
        }

        public override string GetDescription()
        {
            var description = base.GetDescription();
            var traits = _applyOnEquip.Where(t => t).Select(t => t.GetDescription()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (traits.Length > 0)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    description = $"{description}{Environment.NewLine}{Environment.NewLine}";
                }

                foreach (var trait in traits)
                {
                    description = string.IsNullOrEmpty(description) ? trait : $"{description}{Environment.NewLine}{trait}";
                }
            }

            return description;
        }
    }
}