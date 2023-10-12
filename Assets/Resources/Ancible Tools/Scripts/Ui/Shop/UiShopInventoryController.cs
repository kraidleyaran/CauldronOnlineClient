using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Shop
{
    public class UiShopInventoryController : MonoBehaviour
    {
        [SerializeField] private ShopInventoryType _type;
        [SerializeField] private UiShopItemController _shopItemTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private int _maxRows = 4;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;

        private Dictionary<Vector2Int, UiShopItemController> _controllers = new Dictionary<Vector2Int, UiShopItemController>();
        private DataRow<ShopItem>[] _rows = new DataRow<ShopItem>[0];

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition;
        private GameObject _inventoryOwner = null;

        private UiSellStackWindow _sellWindow = null;

        private bool _active = false;
        private bool _hover = false;

        public void Setup(GameObject shopOwner)
        {
            _inventoryOwner = shopOwner;
            RefreshInventory();
            _upArrow.gameObject.SetActive(false);
            _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _rows.Length - _maxRows + 1);
            SubscribeToMessages();
        }

        public void SetActive(bool active, int cursorY = 0)
        {
            if (!_active && active)
            {
                if (_controllers.Count > 0)
                {
                    _active = true;
                    _cursorPosition.y = cursorY;
                    if (_controllers.TryGetValue(_cursorPosition, out var controller))
                    {
                        controller.SetCursor(_cursor);
                    }
                    else
                    {
                        controller = _controllers.OrderBy(kv => (kv.Key - _cursorPosition).sqrMagnitude).FirstOrDefault().Value;
                        controller.SetCursor(_cursor);
                    }
                    gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
                }
                else
                {
                    _active = false;
                    UiShopWindow.SetActiveShop(_type == ShopInventoryType.Player ? ShopInventoryType.Shop : ShopInventoryType.Player, cursorY);
                }
                
            }
            else if (_active && !active)
            {
                _active = false;
                if (_controllers.TryGetValue(_cursorPosition, out var controller))
                {
                    controller.SetHover(false);
                }
                gameObject.Unsubscribe<UpdateInputStateMessage>();
            }

            _cursor.gameObject.SetActive(_active);
        }
        

        private void RefreshInventory()
        {
            var queryShopMsg = MessageFactory.GenerateQueryShopMsg();
            queryShopMsg.DoAfter = UpdateInventory;
            gameObject.SendMessageTo(queryShopMsg, _inventoryOwner);
            MessageFactory.CacheMessage(queryShopMsg);
        }

        private void UpdateInventory(ShopItem[] items)
        {
            var ordered = items.OrderBy(o => o.Item.DisplayName).ToArray();
            var dataItems = new List<DataRow<ShopItem>>();
            var datarow = new DataRow<ShopItem>();
            dataItems.Add(datarow);
            foreach (var item in ordered)
            {
                datarow.Items.Add(item);
                if (datarow.Items.Count >= _grid.constraintCount)
                {

                    var available = dataItems.Count * _grid.constraintCount;
                    if (available < items.Length)
                    {
                        datarow = new DataRow<ShopItem>();
                        dataItems.Add(datarow);
                    }
                }
            }

            _rows = dataItems.ToArray();
            RefreshRows();
        }

        private void RefreshRows()
        {
            _cursor.transform.SetParent(transform);
            _cursor.gameObject.SetActive(false);

            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                controller.SetHover(false);
                Destroy(controller.gameObject);
            }
            _controllers.Clear();

            if (_rows.Length > 0)
            {
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
                        var controller = Instantiate(_shopItemTemplate, _grid.transform);
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
                    setCursorController.SetHover(_hover);
                }
                else
                {
                    var closest = _controllers.OrderBy(c => (c.Value.Position - _cursorPosition).sqrMagnitude).FirstOrDefault();
                    if (closest.Value)
                    {
                        _cursorPosition = closest.Key;
                        closest.Value.SetCursor(_cursor);
                        closest.Value.SetHover(_hover);
                    }

                }
            }
            else
            {
                _cursor.gameObject.SetActive(false);
            }
        }

        private void SellStackFinished(WorldItem item, int stack, bool completed)
        {
            if (completed)
            {
                var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                removeItemMsg.Item = item;
                removeItemMsg.Stack = stack;
                removeItemMsg.Update = true;
                gameObject.SendMessageTo(removeItemMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(removeItemMsg);

                var addGoldMsg = MessageFactory.GenerateAddGoldMsg();
                addGoldMsg.Amount = stack * item.SellValue;
                gameObject.SendMessageTo(addGoldMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(addGoldMsg);
            }

            StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                UiShopWindow.CloseSellStack();
                _sellWindow = null;
            }));

        }

        private void SubscribeToMessages()
        {
            if (_type == ShopInventoryType.Player)
            {
                gameObject.Subscribe<PlayerInventoryUpdatedMessage>(PlayerInventoryUpdated);
            }
        }

        private void PlayerInventoryUpdated(PlayerInventoryUpdatedMessage msg)
        {
            RefreshInventory();
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (_active && !_sellWindow)
            {
                var buttonPushed = false;
                if (!msg.Previous.Green && msg.Current.Green)
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var controller))
                    {
                        switch (_type)
                        {
                            case ShopInventoryType.Player:
                                if (controller.ShopItem.Stack > 1)
                                {
                                    _sellWindow = UiShopWindow.ShowSellStack(controller.ShopItem, SellStackFinished);
                                }
                                else
                                {
                                    var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                                    removeItemMsg.Item = controller.ShopItem.Item;
                                    removeItemMsg.Stack = 1;
                                    removeItemMsg.Update = false;
                                    gameObject.SendMessageTo(removeItemMsg, ObjectManager.Player);
                                    MessageFactory.CacheMessage(removeItemMsg);

                                    var addGoldMsg = MessageFactory.GenerateAddGoldMsg();
                                    addGoldMsg.Amount = controller.ShopItem.Cost;
                                    gameObject.SendMessageTo(addGoldMsg, ObjectManager.Player);
                                    MessageFactory.CacheMessage(addGoldMsg);
                                }

                                buttonPushed = true;
                                break;
                            case ShopInventoryType.Shop:
                                var playerHasGold = false;
                                var queryGoldMsg = MessageFactory.GenerateQueryGoldMsg();
                                queryGoldMsg.DoAfter = gold => playerHasGold =  gold >= controller.ShopItem.Cost;
                                gameObject.SendMessageTo(queryGoldMsg, ObjectManager.Player);
                                MessageFactory.CacheMessage(queryGoldMsg);

                                if (playerHasGold)
                                {
                                    var removeGoldMsg = MessageFactory.GenerateRemoveGoldMsg();
                                    removeGoldMsg.Amount = controller.ShopItem.Cost;
                                    gameObject.SendMessageTo(removeGoldMsg, ObjectManager.Player);
                                    MessageFactory.CacheMessage(removeGoldMsg);

                                    var addItemMsg = MessageFactory.GenerateAddItemMsg();
                                    addItemMsg.Item = controller.ShopItem.Item;
                                    addItemMsg.Stack = controller.ShopItem.Stack;
                                    gameObject.SendMessageTo(addItemMsg, ObjectManager.Player);
                                    MessageFactory.CacheMessage(addItemMsg);
                                    buttonPushed = true;
                                }

                                
                                break;
                        }
                    }

                }
                else if (!msg.Previous.Info && msg.Current.Info)
                {
                    _hover = !_hover;
                    if (_controllers.TryGetValue(_cursorPosition, out var selected))
                    {
                        selected.SetHover(_hover);
                    }
                }

                if (!buttonPushed)
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
                        if (_controllers.TryGetValue(_cursorPosition, out var selectedController))
                        {
                            selectedController.SetHover(false);
                        }
                        if (_controllers.TryGetValue(direction + _cursorPosition, out var itemController))
                        {
                            _cursorPosition = itemController.Position;
                            itemController.SetCursor(_cursor);
                            itemController.SetHover(_hover);
                        }
                        else if (direction.y > 0 && _dataRowPosition + 1 < _rows.Length - _maxRows + 1)
                        {
                            _dataRowPosition++;
                            RefreshRows();
                        }
                        else if (direction.y < 0 && _dataRowPosition > 0)
                        {
                            _dataRowPosition--;
                            RefreshRows();
                        }
                        else
                        {
                            switch (_type)
                            {
                                case ShopInventoryType.Player:
                                    if (direction.x > 0)
                                    {
                                        var currentRow = _rows[_cursorPosition.y + _dataRowPosition];
                                        if (_cursorPosition.y >= currentRow.Items.Count - 1)
                                        {
                                            UiShopWindow.SetActiveShop(ShopInventoryType.Shop, _cursorPosition.y);
                                        }
                                    }
                                    break;
                                case ShopInventoryType.Shop:
                                    if (direction.x < 0 && _cursorPosition.x <= 0)
                                    {
                                        UiShopWindow.SetActiveShop(ShopInventoryType.Player, _cursorPosition.y);
                                    }
                                    break;
                            }
                        }

                        _upArrow.gameObject.SetActive(_dataRowPosition > 0);
                        _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _rows.Length - _maxRows + 1);
                    }
                }
                
            }

        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }


}