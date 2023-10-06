using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Zones
{
    [Serializable]
    public class PremadeLoadoutSlot
    {
        public virtual LoadoutSlotData GetData(int slot)
        {
            return new LoadoutSlotData();
        }
    }

    [Serializable]
    public class PremadeItemSlot : PremadeLoadoutSlot
    {
        public ActionItem Item;
        public int Stack = 1;

        public override LoadoutSlotData GetData(int slot)
        {
            return new LoadoutSlotData {Item = Item.name, Stack = Stack, Ability = Item.Ability.name, Slot = slot};
        }
    }

    [Serializable]
    public class PremadeAbilitySlot : PremadeLoadoutSlot
    {
        public WorldAbility Ability;

        public override LoadoutSlotData GetData(int slot)
        {
            
            return new LoadoutSlotData{Ability = Ability.name, Slot = slot};
        }
    }


}