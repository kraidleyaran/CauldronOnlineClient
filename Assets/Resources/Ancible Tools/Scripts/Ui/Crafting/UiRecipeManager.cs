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
    public class UiRecipeManager : MonoBehaviour
    {
        [SerializeField] private UiItemRecipeController _recipeTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows = 4;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;

        private DataRow<ActiveRecipe>[] _items = new DataRow<ActiveRecipe>[0];
        private Dictionary<Vector2Int, UiItemRecipeController> _controllers = new Dictionary<Vector2Int, UiItemRecipeController>();

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;
        private bool _hover = false;
        private bool _active = false;

        public void Setup(ItemRecipeData[] recipes)
        {
            var itemRecipes = new List<ActiveRecipe>();
            foreach (var recipe in recipes)
            {
                var item = ItemFactory.GetItemByName(recipe.Item);
                if (item)
                {
                    itemRecipes.Add(new ActiveRecipe(item, recipe.Recipe, recipe.Stack, recipe.Gold));
                }
            }

            var ordered = itemRecipes.OrderBy(i => i.Item.GetDisplayName()).ThenBy(i => i.Item.Quality).ToArray();
            UpdateRecipes(ordered);
            SubscribeToMessages();
        }

        public void SetActive(bool active)
        {
            _active = active;
            _cursor.gameObject.SetActive(_active);
            if (_controllers.TryGetValue(_cursorPosition, out var controller))
            {
                controller.SetHovered(_active && _hover);
            }
        }

        public void CraftSelected()
        {
            if (_controllers.TryGetValue(_cursorPosition, out var controller))
            {
                var addItemMsg = MessageFactory.GenerateAddItemMsg();
                addItemMsg.Item = controller.Recipe.Item;
                addItemMsg.Stack = controller.Recipe.Stack;
                gameObject.SendMessageTo(addItemMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(addItemMsg);
            }
        }

        private void UpdateRecipes(ActiveRecipe[] items)
        {
            var dataItems = new List<DataRow<ActiveRecipe>>();
            var datarow = new DataRow<ActiveRecipe>();
            dataItems.Add(datarow);
            foreach (var item in items)
            {
                datarow.Items.Add(item);
                if (datarow.Items.Count >= _grid.constraintCount)
                {

                    var available = dataItems.Count * _grid.constraintCount;
                    if (available < items.Length)
                    {
                        datarow = new DataRow<ActiveRecipe>();
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
                        var controller = Instantiate(_recipeTemplate, _grid.transform);
                        controller.Setup(item);
                        controller.Position = position;
                        _controllers.Add(position, controller);
                        position.x++;
                    }

                    position.x = 0;
                    position.y++;
                }

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
            else
            {
                _cursor.gameObject.SetActive(false);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Info && msg.Current.Info)
            {
                _hover = !_hover;
                if (_active)
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var controller))
                    {
                        controller.SetHovered(_hover);
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

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }


    }


}