using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Inventory
{
    public class UiInventoryPlayerMenu : UiPlayerMenu
    {
        [SerializeField] private UiInventoryItemController _itemTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows = 4;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;


        private DataRow<ItemStack>[] _items = new DataRow<ItemStack>[0];
        private Dictionary<Vector2Int, UiInventoryItemController> _controllers = new Dictionary<Vector2Int, UiInventoryItemController>();

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;

        void Awake()
        {
            RefreshInventory();
            _upArrow.gameObject.SetActive(false);
            _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _items.Length - _maxRows + 1);
            SubscribeToMessages();
        }

        private void RefreshInventory()
        {
            var queryInventoryMsg = MessageFactory.GenerateQueryInventoryMsg();
            queryInventoryMsg.DoAfter = UpdateInventory;
            gameObject.SendMessageTo(queryInventoryMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryInventoryMsg);
        }

        private void RefreshRows()
        {
            _cursor.transform.SetParent(transform);
            _cursor.gameObject.SetActive(false);

            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                controller.Destroy();
                Destroy(controller.gameObject);
            }
            _controllers.Clear();

            if (_items.Length > 0)
            {
                var startingRow = (_items.Length - _maxRows) < 0 ? 0 : _dataRowPosition;
                var endingRow = _items.Length - 1;
                if (_items.Length > _maxRows)
                {
                    endingRow = Mathf.Min(_dataRowPosition + _maxRows - 1, _items.Length - 1);
                }
                

                var position = Vector2Int.zero;
                for (var row = startingRow; row <= endingRow; row++)
                {
                    var itemRow = _items[row];
                    foreach (var item in itemRow.Items)
                    {
                        var controller = Instantiate(_itemTemplate, _grid.transform);
                        controller.Setup(item, position);
                        _controllers.Add(position, controller);
                        position.x++;
                    }

                    position.x = 0;
                    position.y++;
                }

                if (_controllers.TryGetValue(_cursorPosition, out var setCursorController))
                {
                    setCursorController.SetCursor(_cursor);
                }
                else
                {
                    var closest = _controllers.OrderBy(c => (c.Value.Position - _cursorPosition).sqrMagnitude).FirstOrDefault();
                    if (closest.Value)
                    {
                        _cursorPosition = closest.Key;
                        closest.Value.SetCursor(_cursor);
                    }

                }
            }
            else
            {
                _cursor.gameObject.SetActive(false);
            }
        }

        private void UpdateInventory(ItemStack[] items)
        {
            var dataItems = new List<DataRow<ItemStack>>();
            var datarow = new DataRow<ItemStack>();
            dataItems.Add(datarow);
            foreach (var item in items)
            {
                datarow.Items.Add(item);
                if (datarow.Items.Count >= _grid.constraintCount)
                {
                    
                    var available = dataItems.Count * _grid.constraintCount;
                    if (available < items.Length)
                    {
                        datarow = new DataRow<ItemStack>();
                        dataItems.Add(datarow);
                    }
                }
            }

            _items = dataItems.ToArray();
            RefreshRows();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<PlayerInventoryUpdatedMessage>(PlayerInventoryUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var buttonPressed = false;
            if (_controllers.TryGetValue(_cursorPosition, out var controller))
            {
                if (!msg.Previous.Green && msg.Current.Green && controller.Item.Item.Type == ItemType.Armor && controller.Item.Item is ArmorItem armor)
                {
                    var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                    removeItemMsg.Item = armor;
                    removeItemMsg.Stack = 1;
                    removeItemMsg.Update = false;
                    gameObject.SendMessageTo(removeItemMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(removeItemMsg);

                    var equipArmorItemMsg = MessageFactory.GenerateEquipArmorItemMsg();
                    equipArmorItemMsg.Item = armor;
                    gameObject.SendMessageTo(equipArmorItemMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(equipArmorItemMsg);
                    buttonPressed = true;
                    gameObject.SendMessage(PlayerInventoryUpdatedMessage.INSTANCE);
                }
                else
                {
                    controller.SetHover(msg.Current.Info);
                }
                
            }

            if (!buttonPressed)
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
                    if (_controllers.TryGetValue(direction + _cursorPosition, out var itemController))
                    {
                        _cursorPosition = itemController.Position;
                        itemController.SetCursor(_cursor);
                    }
                    else if (direction.y > 0 && _dataRowPosition + 1 < _items.Length - _maxRows + 1)
                    {
                        _dataRowPosition++;
                        RefreshRows();
                    }
                    else if (direction.y < 0 && _dataRowPosition > 0)
                    {
                        _dataRowPosition--;
                        RefreshRows();
                    }

                    _upArrow.gameObject.SetActive(_dataRowPosition > 0);
                    _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _items.Length - _maxRows + 1);
                }
            }
            
        }

        private void PlayerInventoryUpdated(PlayerInventoryUpdatedMessage msg)
        {
            RefreshInventory();
        }
    }
}