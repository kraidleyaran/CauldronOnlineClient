using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Shop
{
    public class UiSellStackWindow : UiWindowBase
    {
        public const int TEN_STACK = 10;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Image _icon;
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _valueText;
        [SerializeField] private Text _stackText;
        [SerializeField] private Text _totalText;

        private Action<WorldItem, int, bool> _doAfter = null;
        private ShopItem _shopItem = null;

        private int _sellStack = 0;

        public void Setup(ShopItem item, Action<WorldItem, int, bool> doAfter)
        {
            _shopItem = item;
            _icon.sprite = _shopItem.Item.Sprite.Sprite;
            _itemNameText.text = _shopItem.Item.DisplayName;
            _sellStack = _shopItem.Stack;
            _stackText.text = $"{_sellStack:n0}";
            _valueText.text = $"{_shopItem.Item.SellValue:n0}";
            _doAfter = doAfter;
            RefreshTotal();
            SubscribeToMessages();
        }

        private void RefreshTotal()
        {
            _totalText.text = $"{_sellStack * _shopItem.Item.SellValue:n0}";
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Green && msg.Current.Green)
            {
                _doAfter.Invoke(_shopItem.Item, _sellStack, true);
                gameObject.UnsubscribeFromAllMessages();
            }
            else if (!msg.Previous.Red && msg.Current.Red)
            {
                _doAfter.Invoke(_shopItem.Item, 0, false);
                gameObject.UnsubscribeFromAllMessages();
            }
            else
            {
                var changeStack = 0;
                if (!msg.Previous.Blue && msg.Current.Blue && _sellStack < _shopItem.Stack)
                {
                    if (_sellStack + TEN_STACK <= _shopItem.Stack)
                    {
                        changeStack = TEN_STACK;
                    }
                    else
                    {
                        changeStack = _shopItem.Stack - _sellStack;
                    }
                }
                else if (!msg.Previous.Yellow && msg.Current.Yellow && _sellStack > 0)
                {
                    if (_sellStack - TEN_STACK >= 0)
                    {
                        changeStack = -TEN_STACK;
                    }
                    else
                    {
                        changeStack = -_sellStack;
                    }
                }
                else if (!msg.Previous.Up && msg.Current.Up && _sellStack < _shopItem.Stack)
                {
                    changeStack = 1;
                }
                else if (!msg.Previous.Down && msg.Current.Down && _sellStack > 0)
                {
                    changeStack = -1;
                }

                if (changeStack != 0)
                {
                    _sellStack += changeStack;
                    _stackText.text = $"{_sellStack:n0}";
                    RefreshTotal();
                }
            }
        }
    }
}