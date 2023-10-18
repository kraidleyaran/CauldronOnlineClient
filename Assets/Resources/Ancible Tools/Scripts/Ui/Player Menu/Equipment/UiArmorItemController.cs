using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Equipment
{
    public class UiArmorItemController : MonoBehaviour
    {
        public ArmorSlot Slot;
        public Vector2Int Position;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _unequippedImage;
        [SerializeField] private RectTransform _cursorPosition;

        public EquippedArmorItemInstance Item;

        private bool _hovered = false;

        public void Setup(EquippedArmorItemInstance item)
        {
            Item = item;
            var itemEquipped = item != null;
            _itemImage.sprite = itemEquipped ? Item.Item.Sprite.Sprite : null;
            _itemImage.gameObject.SetActive(itemEquipped);
            _frameImage.color = itemEquipped ? ItemFactory.GetQualityColor(Item.Item) : Color.white;
            _unequippedImage.gameObject.SetActive(!itemEquipped);
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
            cursor.gameObject.SetActive(true);
        }

        public void SetHover(bool hovered)
        {
            if (!_hovered && hovered)
            {
                if (Item != null)
                {
                    UiHoverInfoManager.SetHoverInfo(gameObject, Item.Item.GetDisplayName(), Item.Item.GetDescription(), Item.Item.Sprite.Sprite, transform.position.ToVector2());
                }
                else
                {
                    UiHoverInfoManager.SetHoverInfo(gameObject, Slot.ToString(), "Equip an item from your Inventory", _unequippedImage.sprite, transform.position.ToVector2());
                }
                
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