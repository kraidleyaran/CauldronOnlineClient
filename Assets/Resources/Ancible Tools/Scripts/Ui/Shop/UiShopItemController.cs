using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Shop
{
    public class UiShopItemController : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Text _stacktext;
        [SerializeField] private RectTransform _cursorPosition;

        public ShopItem ShopItem;
        public Vector2Int Position;

        private bool _hovered = false;

        public void Setup(ShopItem shopItem, Vector2Int position)
        {
            ShopItem = shopItem;
            _itemIcon.sprite = ShopItem.Item.Sprite.Sprite;
            _stacktext.text = ShopItem.Stack > 1 ? $"x{ShopItem.Stack}" : string.Empty;
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.transform.gameObject.SetActive(true);
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void SetHover(bool hovered)
        {
            if (!_hovered && hovered)
            {
                _hovered = true;
                UiHoverInfoManager.SetHoverInfo(gameObject, ShopItem.Item.DisplayName, ShopItem.Item.GetDescription(), ShopItem.Item.Sprite.Sprite, transform.position.ToVector2(), ShopItem.Cost);
            }
            else if (_hovered && !hovered)
            {
                _hovered = false;
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
        }
    }
}