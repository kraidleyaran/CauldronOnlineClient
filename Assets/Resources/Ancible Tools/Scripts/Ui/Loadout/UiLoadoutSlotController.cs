using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Loadout
{
    public class UiLoadoutSlotController : MonoBehaviour
    {
        [SerializeField] private Image _sprite;
        [SerializeField] private Text _usesText;
        [SerializeField] private Image _outline;
        [SerializeField] private Image _outlineMask;
        [SerializeField] private RectTransform _cursorPosition;

        public LoadoutSlot Item;
        public int Slot;

        private bool _hovered = false;

        public void Setup(LoadoutSlot item, int slot)
        {
            Item = item;
            Slot = slot;
            if (item.IsEmpty)
            {
                _sprite.sprite = null;
                _sprite.gameObject.SetActive(false);
                _outline.gameObject.SetActive(false);
            }
            else
            {
                _sprite.sprite = item.Icon;
                _sprite.gameObject.SetActive(true);
                var showOutline = Item.EquippedItem && Item.EquippedItem.Quality != ItemQuality.Normal;
                _outline.gameObject.SetActive(showOutline);
                if (showOutline)
                {
                    _outline.color = ItemFactory.GetQualityColor(Item.EquippedItem);
                    _outlineMask.sprite = Item.EquippedItem.Sprite.Sprite;
                }
            }
            RefreshUses();
        }

        public void RefreshUses()
        {
            if (!Item.IsEmpty && (Item.EquippedItem && Item.EquippedItem.UseStack || Item.Ability.RequiredResources.Length > 0 || Item.Ability.ManaCost > 0))
            {
                _usesText.text = $"{Item.GetUses(ObjectManager.Player)}";
            }
            else
            {
                _usesText.text = string.Empty;
            }
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
                if (Item.IsEmpty)
                {
                    UiHoverInfoManager.SetHoverInfo(gameObject, "Empty", "Equip from your available loadout", null, _cursorPosition.transform.position.ToVector2());
                }
                else
                {
                    UiHoverInfoManager.SetHoverInfo(gameObject,
                        Item.EquippedItem ? Item.EquippedItem.GetDisplayName() : Item.Ability.DisplayName,
                        Item.GetDescription(), Item.Icon, _cursorPosition.transform.position.ToVector2());
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