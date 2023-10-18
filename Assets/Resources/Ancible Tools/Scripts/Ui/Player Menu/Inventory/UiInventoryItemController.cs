using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Inventory
{
    public class UiInventoryItemController : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _framgeImage;
        [SerializeField] private Text _stackText;
        [SerializeField] private RectTransform _cursorPosition;

        public ItemStack Item;
        public Vector2Int Position;

        private bool _hovered = false;

        public void Setup(ItemStack item, Vector2Int position)
        {
            Item = item;
            Position = position;
            _framgeImage.color = ItemFactory.GetQualityColor(Item.Item);
            _iconImage.sprite = item.Item.Sprite.Sprite;
            _stackText.text = item.Stack > 1 ? $"x{item.Stack}" : string.Empty;
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.gameObject.SetActive(true);
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void SetHover(bool hovered)
        {
            if (!_hovered && hovered)
            {
                UiHoverInfoManager.SetHoverInfo(gameObject, Item.Item.GetDisplayName(), Item.Item.GetDescription(), Item.Item.Sprite.Sprite, transform.position.ToVector2(), Item.Item.SellValue);
            }
            else if (_hovered && !hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
            _hovered = hovered;
        }

        public void Destroy()
        {
            Item = null;
            _iconImage.sprite = null;
            _stackText.text = string.Empty;
            if (_hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
        }
    }
}