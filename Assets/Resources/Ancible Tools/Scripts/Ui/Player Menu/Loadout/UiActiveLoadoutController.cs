using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Loadout;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu
{
    public class UiActiveLoadoutController : MonoBehaviour
    {
        [SerializeField] private UiLoadoutSlotController[] _slots = new UiLoadoutSlotController[0];
        [SerializeField] private GameObject _cursor = null;

        private bool _active = false;
        private bool _hovered = false;

        private int _index = 0;

        void Awake()
        {
            RefreshPlayerLoadout();
            _cursor.gameObject.SetActive(false);
            SubscribeToMessages();
        }

        public void SetActive(bool active)
        {
            _active = active;
            _cursor.gameObject.SetActive(_active);
            if (_active)
            {
                _slots[_index].SetCursor(_cursor);
            }
            _slots[_index].SetHovered(_active && _hovered);
        }

        private void RefreshPlayerLoadout()
        {
            var queryLoadoutMsg = MessageFactory.GenerateQueryLoadoutMsg();
            queryLoadoutMsg.DoAfter = UpdateLoadout;
            gameObject.SendMessageTo(queryLoadoutMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryLoadoutMsg);
        }

        private void UpdateLoadout(LoadoutSlot[] loadout)
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                var controller = _slots[i];
                if (loadout.Length > i)
                {
                    controller.Setup(loadout[i], i);
                }
                else
                {
                    controller.Setup(null, i);
                }
            }
            if (_index < _slots.Length)
            {
                _slots[_index].SetHovered(_hovered && _active);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<PlayerLoadoutUpdatedMessage>(PlayerLoadoutUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Info && msg.Current.Info)
            {
                _hovered = !_hovered;
                _slots[_index].SetHovered(_hovered && _active);
            }
            if (_active)
            {
                var slotButtonPushed = -1;
                if (msg.Current.Loadout.Contains(true))
                {
                    for (var i = 0; i < msg.Previous.Loadout.Length; i++)
                    {
                        if (!msg.Previous.Loadout[i] && msg.Current.Loadout[i])
                        {
                            slotButtonPushed = i;
                            break;
                        }

                    }
                }

                if (slotButtonPushed >= 0)
                {
                    var controller = _slots[_index];
                    if (slotButtonPushed == _index)
                    {
                        if (!controller.Item.IsEmpty)
                        {
                            var unequipLoadoutSlotMsg = MessageFactory.GenerateUnequipLoadoutSlotMsg();
                            unequipLoadoutSlotMsg.Slot = _index;
                            gameObject.SendMessageTo(unequipLoadoutSlotMsg, ObjectManager.Player);
                            MessageFactory.CacheMessage(unequipLoadoutSlotMsg);
                        }
                    }
                    else if (!controller.Item.IsEmpty)
                    {
                        if (controller.Item.EquippedItem)
                        {
                            if (_slots[slotButtonPushed].Item.IsEmpty)
                            {
                                var item = controller.Item.EquippedItem;
                                var stack = controller.Item.Stack;

                                var unequipLoadoutSlotMsg = MessageFactory.GenerateUnequipLoadoutSlotMsg();
                                unequipLoadoutSlotMsg.Slot = _index;
                                gameObject.SendMessageTo(unequipLoadoutSlotMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(unequipLoadoutSlotMsg);

                                var equipItemToLoadoutSlotMsg = MessageFactory.GeneratEquipItemToLoadoutSlotMsg();
                                equipItemToLoadoutSlotMsg.Item = item;
                                equipItemToLoadoutSlotMsg.Stack = stack;
                                equipItemToLoadoutSlotMsg.Slot = slotButtonPushed;
                                gameObject.SendMessageTo(equipItemToLoadoutSlotMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(equipItemToLoadoutSlotMsg);
                            }
                            else
                            {
                                var swapLoadoutSlotsMsg = MessageFactory.GenerateSwapLoadoutSlotsMsg();
                                swapLoadoutSlotsMsg.SlotA = slotButtonPushed;
                                swapLoadoutSlotsMsg.SlotB = _index;
                                gameObject.SendMessageTo(swapLoadoutSlotsMsg, ObjectManager.Player);
                            }

                        }
                        else
                        {
                            if (_slots[slotButtonPushed].Item.IsEmpty)
                            {
                                var ability = controller.Item.Ability;

                                var unequipLoadoutSlotMsg = MessageFactory.GenerateUnequipLoadoutSlotMsg();
                                unequipLoadoutSlotMsg.Slot = _index;
                                gameObject.SendMessageTo(unequipLoadoutSlotMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(unequipLoadoutSlotMsg);

                                var equipAbilityToSlotMsg = MessageFactory.GenerateEquipAbilityToLoadoutSlotMsg();
                                equipAbilityToSlotMsg.Ability = ability;
                                equipAbilityToSlotMsg.Slot = slotButtonPushed;
                                gameObject.SendMessageTo(equipAbilityToSlotMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(equipAbilityToSlotMsg);
                            }
                            else
                            {
                                var swapLoadoutSlotsMsg = MessageFactory.GenerateSwapLoadoutSlotsMsg();
                                swapLoadoutSlotsMsg.SlotA = slotButtonPushed;
                                swapLoadoutSlotsMsg.SlotB = _index;
                                gameObject.SendMessageTo(swapLoadoutSlotsMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(swapLoadoutSlotsMsg);
                            }
                        }
                    }
                }
                else
                {
                    if (!msg.Previous.Left && msg.Current.Left)
                    {
                        _slots[_index].SetHovered(false);
                        _index = _index > 0 ? _index - 1 : _slots.Length - 1;
                        _slots[_index].SetCursor(_cursor);
                        _slots[_index].SetHovered(_hovered);
                    }
                    else if (!msg.Previous.Right && msg.Current.Right)
                    {
                        _slots[_index].SetHovered(false);
                        _index = _index < _slots.Length - 1 ? _index + 1 : 0;
                        _slots[_index].SetCursor(_cursor);
                        _slots[_index].SetHovered(_hovered);
                    }
                }
            }


        }

        private void PlayerLoadoutUpdated(PlayerLoadoutUpdatedMessage msg)
        {
            RefreshPlayerLoadout();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}