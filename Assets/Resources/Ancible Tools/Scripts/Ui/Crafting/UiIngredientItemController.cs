using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Crafting
{
    public class UiIngredientItemController : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _itemFrame;
        [SerializeField] private Text _requiredStackText;
        [SerializeField] private RectTransform _cursorPosition;

        public WorldItem Item = null;
        public Vector2Int Position = Vector2Int.zero;

        public bool RequirementFulfilled => _playerStack >= _stack;

        private int _stack = 0;
        private int _playerStack = 0;

        private bool _hovered = false;

        public void Setup(WorldItem item, int stack)
        {
            Item = item;
            _itemIcon.sprite = item.Sprite.Sprite;
            _itemFrame.color = ItemFactory.GetQualityColor(Item);
            _stack = stack;
            _requiredStackText.text = $"{_playerStack}/{_stack}";
        }

        public void RefreshPlayerStack(int playerStack)
        {
            _playerStack = playerStack;
            _requiredStackText.text = $"{_playerStack}/{_stack}";
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void SetHovered(bool hovered)
        {
            if (!_hovered && hovered)
            {
                var description = Item.GetDescription();
                description = $"{description}{Environment.NewLine}{_playerStack}/{_stack}";
                UiHoverInfoManager.SetHoverInfo(gameObject, Item.GetDisplayName(), description, Item.Sprite.Sprite, _cursorPosition.transform.position.ToVector2());
            }
            else if (_hovered && !hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }

            _hovered = hovered;
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
        }
    }
}