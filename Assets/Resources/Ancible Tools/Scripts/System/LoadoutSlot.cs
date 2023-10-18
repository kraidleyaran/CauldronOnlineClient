using System;
using System.Linq;
using System.Runtime.InteropServices;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class LoadoutSlot
    {
        public string DisplayName;
        public WorldAbility Ability;
        public ActionItem EquippedItem;
        public Sprite Icon;
        public int Stack = 1;

        public bool IsEmpty => !Ability;

        public void Setup(WorldAbility ability)
        {
            Ability = ability;
            EquippedItem = null;
            DisplayName = Ability.DisplayName;
            Icon = ability.Icon;
        }

        public void Setup(ActionItem item, int stack = 1)
        {
            Ability = item.Ability;
            EquippedItem = item;
            DisplayName = EquippedItem.DisplayName;
            Icon = item.Sprite.Sprite;
            Stack = stack;
        }

        public void Use(GameObject owner)
        {
            if (Ability)
            {
                if (EquippedItem && EquippedItem.UseStack)
                {
                    Stack--;
                }
                var useAbilityMsg = MessageFactory.GenerateUseAbilityMsg();
                useAbilityMsg.Ability = Ability;
                useAbilityMsg.Ids = new string[0];
                owner.SendMessageTo(useAbilityMsg, owner);
                MessageFactory.CacheMessage(useAbilityMsg);
            }
        }

        public void Equip(GameObject owner)
        {
            if (EquippedItem)
            {
                var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                removeItemMsg.Item = EquippedItem;
                removeItemMsg.Stack = Stack;
                owner.SendMessageTo(removeItemMsg, owner);
                MessageFactory.CacheMessage(removeItemMsg);
            }
        }

        public void Unequip(GameObject owner)
        {
            Ability = null;
            Icon = null;
            if (EquippedItem)
            {
                var addItemMsg = MessageFactory.GenerateAddItemMsg();
                addItemMsg.Item = EquippedItem;
                addItemMsg.Stack = Stack;
                owner.SendMessageTo(addItemMsg, owner);
                MessageFactory.CacheMessage(addItemMsg);

                EquippedItem = null;
                
            }
        }

        public bool CanUse(GameObject owner)
        {
            var canUse = true;
            
            if (EquippedItem && EquippedItem.UseStack)
            {
                canUse = Stack > 0;
            }
            if (Ability.ManaCost > 0)
            {
                var manaReduction = 0;
                var availableMana = 0;
                var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
                queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, vitals, secondaryStats) =>
                {
                    availableMana = vitals.Mana;
                    manaReduction = CombatManager.Settings.CalculateManaReduction(baseStats + bonusStats) + secondaryStats.ManaReduction;
                };
                owner.SendMessageTo(queryCombatStatsMsg, owner);
                MessageFactory.CacheMessage(queryCombatStatsMsg);
                var manaCost = Mathf.Max(1, Ability.ManaCost - manaReduction);
                canUse = availableMana >= manaCost;
            }
            if (canUse && Ability.RequiredResources.Length > 0)
            {
                var playerResourceUses = 0;
                var queryResourceAvailableUsesMsg = MessageFactory.GenerateQueryAvailableResourceUsesMsg();
                queryResourceAvailableUsesMsg.Items = Ability.RequiredResources;
                queryResourceAvailableUsesMsg.DoAfter = resourceUses => playerResourceUses = resourceUses;
                owner.SendMessageTo(queryResourceAvailableUsesMsg, owner);
                MessageFactory.CacheMessage(queryResourceAvailableUsesMsg);

                canUse = playerResourceUses > 0;
            }
            if (canUse && Ability.Cooldown > 0)
            {
                var abilityIsNotOnCooldown = false;
                var queryAbilityCooldownMsg = MessageFactory.GenerateQueryAbilityCooldownMsg();
                queryAbilityCooldownMsg.Ability = Ability;
                queryAbilityCooldownMsg.DoAfter = cooldown => abilityIsNotOnCooldown = cooldown == null;
                owner.SendMessageTo(queryAbilityCooldownMsg, owner);
                MessageFactory.CacheMessage(queryAbilityCooldownMsg);
                canUse = abilityIsNotOnCooldown;
            }
            
            return canUse;

        }

        public LoadoutSlotData GetData(int slot)
        {
            return new LoadoutSlotData {Slot = slot, Ability = Ability.name, Item = EquippedItem ? EquippedItem.name : string.Empty, Stack = Stack};
        }

        public int GetUses(GameObject owner)
        {
            var uses = EquippedItem && EquippedItem.UseStack ? Stack : -1;
            if (Ability.RequiredResources.Length > 0)
            {
                var resourceUses = 0;
                var queryAvailableUses = MessageFactory.GenerateQueryAvailableResourceUsesMsg();
                queryAvailableUses.Items = Ability.RequiredResources;
                queryAvailableUses.DoAfter = available => resourceUses = available;
                owner.SendMessageTo(queryAvailableUses, owner);
                MessageFactory.CacheMessage(queryAvailableUses);


                uses = uses < 0 ? resourceUses : Mathf.Min(uses, resourceUses);
            }

            if (Ability.ManaCost > 0)
            {
                var availableMana = 0;
                var manaReduction = 0;
                var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
                queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, vitals, bonusSecondary) =>
                {
                    availableMana = vitals.Mana;
                    manaReduction = CombatManager.Settings.CalculateManaReduction(baseStats + bonusStats) + bonusSecondary.ManaReduction;
                };
                owner.SendMessageTo(queryCombatStatsMsg, owner);
                MessageFactory.CacheMessage(queryCombatStatsMsg);

                var manaCost = Math.Max(1, Ability.ManaCost - manaReduction);
                var manaUses = availableMana / manaCost;
                uses = uses < 0 ? manaUses : Math.Min(manaUses, uses);
            }
            return uses;
        }

        public string GetDescription()
        {
            return Ability.GetDescription();
        }

        public void Clear()
        {
            Ability = null;
            Icon = null;
            EquippedItem = null;
        }

        
    }
}