﻿using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Loadout;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;
using MessageFactory = Assets.Resources.Ancible_Tools.Scripts.System.MessageFactory;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu
{
    public class UiLoadoutMenu : UiPlayerMenu
    {
        [SerializeField] private UiLoadoutItemController _loadoutSlotTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;
        [SerializeField] private UiActiveLoadoutController _activeLoadoutController;

        private LoadoutSlot[] _available = new LoadoutSlot[0];
        private DataRow<LoadoutSlot>[] _rows = new DataRow<LoadoutSlot>[0];
        private Dictionary<Vector2Int, UiLoadoutItemController> _controllers = new Dictionary<Vector2Int, UiLoadoutItemController>();

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;
        private bool _hover = false;
        private bool _active = true;

        void Awake()
        {
            RefreshSlots();
            _upArrow.gameObject.SetActive(false);
            _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _rows.Length - _maxRows + 1);
            SubscribeToMessages();
        }

        void Start()
        {
            if (_available.Length <= 0)
            {
                _active = false;
                _activeLoadoutController.SetActive(true);
            }
        }

        private static bool IsEquippableItem(ItemStack stack)
        {
            return stack.Item.Type == ItemType.Action;
        }

        private void RefreshSlots()
        {
            var available = new List<LoadoutSlot>();
            var playerItems = new ItemStack[0];

            var queryItemsMsg = MessageFactory.GenerateQueryItemsMessage();
            queryItemsMsg.Query = IsEquippableItem;
            queryItemsMsg.DoAfter = items => playerItems = items;
            queryItemsMsg.StackAll = false;
            gameObject.SendMessageTo(queryItemsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryItemsMsg);

            foreach (var item in playerItems)
            {
                if (item.Item is ActionItem actionItem)
                {
                    var slot = new LoadoutSlot();
                    slot.Setup(actionItem, item.Stack);
                    available.Add(slot);
                }
            }

            var abilities = new WorldAbility[0];

            var queryAbiltiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
            queryAbiltiesMsg.DoAfter = playerAbilities => abilities = playerAbilities;
            gameObject.SendMessageTo(queryAbiltiesMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryAbiltiesMsg);

            foreach (var ability in abilities)
            {
                var slot = new LoadoutSlot();
                slot.Setup(ability);
                available.Add(slot);
            }

            _available = available.ToArray();

            foreach (var row in _rows)
            {
                foreach (var slot in row.Items)
                {
                    slot.Clear();
                }
                row.Items.Clear();
            }
            _rows = new DataRow<LoadoutSlot>[0];

            RefreshRows();
        }

        private void RefreshRows()
        {
            _cursor.transform.SetParent(transform);
            _cursor.gameObject.SetActive(false);

            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                Destroy(controller.gameObject);
            }
            _controllers.Clear();

            if (_available.Length > 0)
            {
                _available = _available.OrderBy(a => a.EquippedItem != null).ThenBy(a => a.Ability.name).ToArray();
                var rows = new List<DataRow<LoadoutSlot>>();
                var dataRow = new DataRow<LoadoutSlot>();
                rows.Add(dataRow);
                foreach (var slot in _available)
                {
                    dataRow.Items.Add(slot);
                    if (dataRow.Items.Count >= _grid.constraintCount)
                    {
                        var available = rows.Count * _grid.constraintCount;
                        if (available < _available.Length)
                        {
                            dataRow = new DataRow<LoadoutSlot>();
                            rows.Add(dataRow);
                        }
                    }
                }

                _rows = rows.ToArray();

                var startingRow = (_rows.Length - _maxRows) < 0 ? 0 : _dataRowPosition;
                var endingRow = _rows.Length - 1;
                if (_rows.Length > _maxRows)
                {
                    endingRow = Mathf.Min(_dataRowPosition + _maxRows - 1, _rows.Length - 1);
                }

                var position = Vector2Int.zero;
                for (var row = startingRow; row <= endingRow; row++)
                {
                    var itemRow = _rows[row];
                    foreach (var item in itemRow.Items)
                    {
                        var controller = Instantiate(_loadoutSlotTemplate, _grid.transform);
                        controller.Setup(item, position);
                        _controllers.Add(position, controller);
                        position.x++;
                    }

                    position.x = 0;
                    position.y++;
                }

                if (_active)
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var setCursorController))
                    {
                        setCursorController.SetCursor(_cursor);
                        setCursorController.SetHovered(_hover);
                    }
                    else if (_controllers.Count > 0)
                    {
                        var closest = _controllers.OrderBy(c => (c.Value.Position - _cursorPosition).sqrMagnitude).FirstOrDefault();
                        if (closest.Value)
                        {
                            _cursorPosition = closest.Key;
                            closest.Value.SetCursor(_cursor);
                            closest.Value.SetHovered(_hover);
                        }
                    }
                    else
                    {
                        _active = false;
                        _cursor.gameObject.SetActive(false);
                        _activeLoadoutController.SetActive(true);
                    }
                }

            }
            else
            {
                //_active = false;
                //_activeLoadoutController.SetActive(true);
                _cursor.gameObject.SetActive(false);
                if (_active)
                {
                    _active = false;
                    _activeLoadoutController.SetActive(true);
                }
            }
            

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<PlayerInventoryUpdatedMessage>(PlayerInventoryUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var loadoutButtonPressed = false;
            if (_active && msg.Current.Loadout.Contains(true))
            {
                for (var i = 0; i < msg.Previous.Loadout.Length; i++)
                {
                    if (!msg.Previous.Loadout[i] && msg.Current.Loadout[i])
                    {
                        if (msg.Current.LeftTrigger)
                        {
                            var unequiLoadoutSlotMsg = MessageFactory.GenerateUnequipLoadoutSlotMsg();
                            unequiLoadoutSlotMsg.Slot = i;
                            gameObject.SendMessageTo(unequiLoadoutSlotMsg, ObjectManager.Player);
                            MessageFactory.CacheMessage(unequiLoadoutSlotMsg);
                            loadoutButtonPressed = true;
                        }
                        else if (_controllers.TryGetValue(_cursorPosition, out var controller))
                        {
                            if (controller.Slot.EquippedItem)
                            {
                                var equipItemToLoadoutSlotMsg = MessageFactory.GeneratEquipItemToLoadoutSlotMsg();
                                equipItemToLoadoutSlotMsg.Item = controller.Slot.EquippedItem;
                                equipItemToLoadoutSlotMsg.Stack = controller.Slot.Stack;
                                equipItemToLoadoutSlotMsg.Slot = i;
                                gameObject.SendMessageTo(equipItemToLoadoutSlotMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(equipItemToLoadoutSlotMsg);

                                gameObject.SendMessage(PlayerInventoryUpdatedMessage.INSTANCE);

                            }
                            else
                            {
                                var equipAbilitytoLoadoutSlotMsg = MessageFactory.GenerateEquipAbilityToLoadoutSlotMsg();
                                equipAbilitytoLoadoutSlotMsg.Ability = controller.Slot.Ability;
                                equipAbilitytoLoadoutSlotMsg.Slot = i;
                                gameObject.SendMessageTo(equipAbilitytoLoadoutSlotMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(equipAbilitytoLoadoutSlotMsg);
                            }
                            loadoutButtonPressed = true;
                            break;
                        }
                    }

                }
            }
            else if (!msg.Previous.Info && msg.Current.Info)
            {
                _hover = !_hover;
                if (_active && _controllers.TryGetValue(_cursorPosition, out var selected))
                {
                    selected.SetHovered(_hover && _active);
                }
            }

            if (!loadoutButtonPressed)
            {
                var direction = Vector2Int.zero;
                if (!msg.Previous.Up && msg.Current.Up)
                {
                    direction.y = -1;
                }
                else if (!msg.Previous.Down && msg.Current.Down)
                {
                    direction.y = 1;
                }
                else if (!msg.Previous.Left && msg.Current.Left)
                {
                    direction.x = -1;
                }
                else if (!msg.Previous.Right && msg.Current.Right)
                {
                    direction.x = 1;
                }

                if (direction != Vector2Int.zero)
                {
                    if (_active)
                    {
                        if (_controllers.TryGetValue(direction + _cursorPosition, out var itemController))
                        {
                            if (_controllers.TryGetValue(_cursorPosition, out var selected))
                            {
                                selected.SetHovered(false);
                            }
                            _cursorPosition = itemController.Position;
                            itemController.SetCursor(_cursor);
                            itemController.SetHovered(_hover);
                        }
                        else if (direction.y > 0)
                        {
                            if (_dataRowPosition + 1 < _rows.Length - _maxRows + 1)
                            {
                                _dataRowPosition++;
                                RefreshRows();
                            }
                            else
                            {
                                _active = false;
                                _cursor.gameObject.SetActive(false);
                                if (_controllers.TryGetValue(_cursorPosition, out var selected))
                                {
                                    selected.SetHovered(false);
                                }
                                _activeLoadoutController.SetActive(true);
                            }
                        }
                        else if (direction.y < 0)
                        {
                            if (_dataRowPosition > 0)
                            {
                                _dataRowPosition--;
                                RefreshRows();
                            }

                        }
                    }
                    else if (direction.y < 0)
                    {
                        _activeLoadoutController.SetActive(false);
                        _active = true;
                        _cursor.gameObject.SetActive(true);
                        if (_controllers.TryGetValue(_cursorPosition, out var controller))
                        {
                            controller.SetCursor(_cursor);
                            controller.SetHovered(_hover);
                        }
                    }
                    

                    _upArrow.gameObject.SetActive(_dataRowPosition > 0);
                    _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _rows.Length - _maxRows + 1);
                }
            }
            
        }

        private void PlayerInventoryUpdated(PlayerInventoryUpdatedMessage msg)
        {
            RefreshSlots();
        }
    }
}