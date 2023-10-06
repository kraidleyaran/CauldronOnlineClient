using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Loadout Trait", menuName = "Ancible Tools/Traits/Player/Player Loadout")]
    public class PlayerLoadoutTrait : Trait
    {
        [SerializeField] private ActionItem[] _startingLoadout = new ActionItem[0];
        [SerializeField] private int _maxLoadoutSlots = 8;

        private Dictionary<int, LoadoutSlot> _loadout = new Dictionary<int, LoadoutSlot>();

        private UnitState _unitState = UnitState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            for (var i = 0; i < _maxLoadoutSlots; i++)
            {
                _loadout.Add(i, new LoadoutSlot());
            }

            var key = 0;
            foreach (var item in _startingLoadout)
            {
                if (key < _loadout.Count)
                {
                    _loadout[key].Setup(item);
                    key++;
                }
                else
                {
                    break;
                }
            }

            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryLoadoutMessage>(QueryLoadout, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EquipAbilityToLoadoutSlotMessage>(EquipAbilityToLoadoutSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<EquipItemToLoadoutSlotMessage>(EquipItemToLoadoutSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnequipLoadoutSlotMessage>(UnequipLoadoutSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetLoadoutMessage>(SetLoadout, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<FillLoadoutStackMessage>(FillLoadoutStack, _instanceId);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (_unitState == UnitState.Active)
            {
                LoadoutSlot slot = null;
                for (var i = 0; i < msg.Current.Loadout.Length; i++)
                {
                    if (msg.Current.Loadout[i] && _loadout.TryGetValue(i, out var loadoutSlot) && !loadoutSlot.IsEmpty && loadoutSlot.CanUse(_controller.transform.parent.gameObject))
                    {
                        slot = loadoutSlot;
                        break;
                    }
                }

                slot?.Use(_controller.transform.parent.gameObject);
            }
        }

        private void QueryLoadout(QueryLoadoutMessage msg)
        {
            msg.DoAfter.Invoke(_loadout.Values.ToArray());
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void EquipItemToLoadoutSlot(EquipItemToLoadoutSlotMessage msg)
        {
            if (_loadout.TryGetValue(msg.Slot, out var loadout))
            {
                if (!loadout.IsEmpty)
                {
                    loadout.Unequip(_controller.transform.parent.gameObject);
                }

                loadout.Setup(msg.Item, msg.Stack);
                loadout.Equip(_controller.transform.parent.gameObject);
                _controller.gameObject.SendMessage(PlayerLoadoutUpdatedMessage.INSTANCE);
            }
        }

        private void EquipAbilityToLoadoutSlot(EquipAbilityToLoadoutSlotMessage msg)
        {
            if (_loadout.TryGetValue(msg.Slot, out var loadout))
            {
                if (!loadout.IsEmpty)
                {
                    loadout.Unequip(_controller.transform.parent.gameObject);
                }

                loadout.Setup(msg.Ability);
                _controller.gameObject.SendMessage(PlayerLoadoutUpdatedMessage.INSTANCE);
            }
        }

        private void UnequipLoadoutSlot(UnequipLoadoutSlotMessage msg)
        {
            if (_loadout.TryGetValue(msg.Slot, out var loadout))
            {
                if (!loadout.IsEmpty)
                {
                    loadout.Unequip(_controller.transform.parent.gameObject);
                }
                _controller.gameObject.SendMessage(PlayerLoadoutUpdatedMessage.INSTANCE);
            }
        }

        private void SetLoadout(SetLoadoutMessage msg)
        {
            foreach (var slot in _loadout)
            {
                slot.Value.Clear();
            }

            foreach (var slotData in msg.Loadout)
            {
                if (_loadout.TryGetValue(slotData.Slot, out var slot))
                {
                    if (!string.IsNullOrEmpty(slotData.Item))
                    {
                        var item = ItemFactory.GetItemByName(slotData.Item);
                        if (item && item is ActionItem action)
                        {
                            slot.Setup(action, slotData.Stack);
                        }
                    }
                    else if (!string.IsNullOrEmpty(slotData.Ability))
                    {
                        var ability = AbilityFactory.GetAbilityByName(slotData.Ability);
                        if (ability)
                        {
                            slot.Setup(ability);
                        }
                    }
                }

            }
        }

        private void FillLoadoutStack(FillLoadoutStackMessage msg)
        {
            var available = _loadout.Values.Where(s => !s.IsEmpty && s.EquippedItem && s.EquippedItem == msg.Item && s.Stack < s.EquippedItem.MaxStack).OrderByDescending(s => s.Stack).ToArray();
            var remainder = msg.Stack;
            if (available.Length > 0)
            {
                foreach (var slot in available)
                {
                    if (slot.Stack + remainder <= slot.EquippedItem.MaxStack)
                    {
                        slot.Stack += remainder;
                        remainder = 0;
                        break;
                    }
                    else
                    {
                        var removed = slot.EquippedItem.MaxStack - slot.Stack;
                        slot.Stack += removed;
                        remainder -= removed;
                        if (remainder <= 0)
                        {
                            break;
                        }
                    }
                }
            }
            msg.DoAfter.Invoke(remainder);
        }
    }
}