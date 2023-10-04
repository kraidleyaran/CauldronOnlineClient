using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu
{
    public class UiLoadoutItemController : MonoBehaviour
    {
        public LoadoutSlot Slot;
        public Vector2Int Position;

        private bool _hovered = false;

        [SerializeField] private Image _iconImage;
        [SerializeField] private RectTransform _cursorPosition;
        [SerializeField] private Text _availableUsesText;

        public void Setup(LoadoutSlot slot, Vector2Int position)
        {
            Slot = slot;
            Position = position;
            if (Slot.IsEmpty)
            {
                _availableUsesText.text = string.Empty;
            }
            else if (Slot.Ability.RequiredResources.Length > 0 || Slot.Ability.ManaCost > 0)
            {
                _availableUsesText.text = $"{Slot.GetUses(ObjectManager.Player)}";
            }
            else
            {
                _availableUsesText.text = string.Empty;
            }
            _iconImage.sprite = Slot.EquippedItem ? Slot.EquippedItem.Sprite.Sprite : Slot.Ability.Icon;
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.gameObject.SetActive(true);
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void SetHovered(bool hovered)
        {
            if (!_hovered && hovered)
            {
                var title = Slot.EquippedItem ? Slot.EquippedItem.DisplayName : Slot.Ability.DisplayName;
                UiHoverInfoManager.SetHoverInfo(gameObject, title, Slot.Ability.GetDescription(), Slot.Icon, transform.position.ToVector2());
            }
            else if (_hovered && !hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }

            _hovered = hovered;
        }
    }
}