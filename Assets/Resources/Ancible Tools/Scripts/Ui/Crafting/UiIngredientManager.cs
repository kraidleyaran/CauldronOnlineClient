using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon.Data.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Crafting
{
    public class UiIngredientManager : MonoBehaviour
    {
        [SerializeField] private UiIngredientItemController _itemTemplate;
        [SerializeField] private Text _costText = null;
        [SerializeField] private Color _missingAmountColor = Color.red;
        [SerializeField] private Color _fulfilledAmountColor = Color.green;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows = 4;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;

        private DataRow<ItemIngredient>[] _items = new DataRow<ItemIngredient>[0];
        private Dictionary<Vector2Int, UiIngredientItemController> _controllers = new Dictionary<Vector2Int, UiIngredientItemController>();
        private WorldItem[] _requiredItems = new WorldItem[0];

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;
        private bool _hover = false;
        private bool _active = false;
        private int _cost = 0;

        void Awake()
        {
            SubscribeToMessages();
        }

        public void Setup(WorldItemStackData[] items, int cost)
        {
            _cursor.transform.SetParent(transform);
            _cursorPosition = Vector2Int.zero;
            _cost = cost;
            if (_controllers.Count > 0)
            {
                var controllers = _controllers.Values.ToArray();
                foreach (var controller in controllers)
                {
                    Destroy(controller.gameObject);
                }
                _controllers = new Dictionary<Vector2Int, UiIngredientItemController>();
            }

            var queryGoldMsg = MessageFactory.GenerateQueryGoldMsg();
            queryGoldMsg.DoAfter = UpdatePlayerGold;
            gameObject.SendMessageTo(queryGoldMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryGoldMsg);
            UpdateIngredients(items);
        }

        public void SetActive(bool active)
        {
            _active = active;
            _cursor.gameObject.SetActive(_active);
            if (_controllers.TryGetValue(_cursorPosition, out var controller))
            {
                controller.SetHovered(_hover && _active);
            }

        }

        public bool CanCraft()
        {
            var canCraft = true;
            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                canCraft = controller.RequirementFulfilled;
                if (!canCraft)
                {
                    break;
                }
            }

            if (canCraft)
            {
                var queryGoldMsg = MessageFactory.GenerateQueryGoldMsg();
                queryGoldMsg.DoAfter = playerGold => canCraft = playerGold >= _cost;
                gameObject.SendMessageTo(queryGoldMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(queryGoldMsg);
            }
            return canCraft;
        }

        public void RemoveItemsFromPlayer()
        {
            var items = new List<ItemIngredient>();
            foreach (var row in _items)
            {
                items.AddRange(row.Items);
            }

            var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
            foreach (var ingredient in items)
            {
                removeItemMsg.Item = ingredient.Item;
                removeItemMsg.Stack = ingredient.Stack;
                removeItemMsg.Update = false;
                gameObject.SendMessageTo(removeItemMsg, ObjectManager.Player);
            }
            MessageFactory.CacheMessage(removeItemMsg);

            if (_cost > 0)
            {
                var removeGoldMsg = MessageFactory.GenerateRemoveGoldMsg();
                removeGoldMsg.Amount = _cost;
                gameObject.SendMessageTo(removeGoldMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(removeGoldMsg);
            }

            
            gameObject.SendMessage(PlayerInventoryUpdatedMessage.INSTANCE);
        }

        private void RefresPlayerItemStacks()
        {
            var queryItemsMsg = MessageFactory.GenerateQueryItemsMessage();
            queryItemsMsg.DoAfter = UpdateItemStacks;
            queryItemsMsg.Query = IsIngredientItem;
            queryItemsMsg.StackAll = true;
            gameObject.SendMessageTo(queryItemsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryItemsMsg);
        }

        private bool IsIngredientItem(ItemStack stack)
        {
            return _requiredItems.Contains(stack.Item);
        }

        private void UpdateItemStacks(ItemStack[] items)
        {
            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                var item = items.FirstOrDefault(s => s.Item == controller.Item);
                controller.RefreshPlayerStack(item?.Stack ?? 0, _missingAmountColor, _fulfilledAmountColor);
            }

            var ordered = controllers.OrderBy(c => !c.RequirementFulfilled).ToArray();
            for (var i = 0; i < ordered.Length; i++)
            {
                ordered[i].transform.SetSiblingIndex(i);
            }
        }

        private void UpdateIngredients(WorldItemStackData[] ingredients)
        {
            var activeIngredients = new List<ItemIngredient>();
            var requiredItems = new List<WorldItem>();
            foreach (var ingredient in ingredients)
            {
                var item = ItemFactory.GetItemByName(ingredient.Item);
                if (item)
                {
                    activeIngredients.Add(new ItemIngredient{Item = item, Stack = ingredient.Stack});
                    if (!requiredItems.Contains(item))
                    {
                        requiredItems.Add(item);
                    }
                }
                
            }

            _requiredItems = requiredItems.ToArray();
            var recipeIngredients = activeIngredients.OrderBy(i => i.Item.GetDisplayName()).ThenBy(i => i.Item.Quality).ToArray();
            var dataItems = new List<DataRow<ItemIngredient>>();
            var datarow = new DataRow<ItemIngredient>();
            dataItems.Add(datarow);
            foreach (var item in recipeIngredients)
            {
                datarow.Items.Add(item);
                if (datarow.Items.Count >= _grid.constraintCount)
                {

                    var available = dataItems.Count * _grid.constraintCount;
                    if (available < ingredients.Length)
                    {
                        datarow = new DataRow<ItemIngredient>();
                        dataItems.Add(datarow);
                    }
                }
            }

            _items = dataItems.ToArray();
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
                        controller.Setup(item.Item, item.Stack);
                        controller.Position = position;
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
                    else
                    {
                        var closest = _controllers.OrderBy(c => (c.Value.Position - _cursorPosition).sqrMagnitude).FirstOrDefault();
                        if (closest.Value)
                        {
                            _cursorPosition = closest.Key;
                            closest.Value.SetCursor(_cursor);
                            closest.Value.SetHovered(_hover);
                        }

                    }
                }
            }
            else
            {
                _cursor.gameObject.SetActive(false);
            }

            RefresPlayerItemStacks();
        }

        private void UpdatePlayerGold(int amount)
        {
            var color = amount >= _cost ? _fulfilledAmountColor : _missingAmountColor;
            _costText.text = $"{StaticMethods.ApplyColorToText($"{_cost:N0}", color)}";
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<PlayerInventoryUpdatedMessage>(PlayerInventoryUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Info && msg.Current.Info)
            {
                _hover = !_hover;
                if (_active)
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var selected))
                    {
                        selected.SetHovered(_hover);
                    }
                }

            }

            if (_active)
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
                        itemController.SetHovered(_hover);
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
            RefresPlayerItemStacks();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}