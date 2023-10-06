using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Loadout
{
    public class UiLoadoutSlotController : MonoBehaviour
    {
        [SerializeField] private Image _sprite;
        [SerializeField] private Text _usesText;

        public LoadoutSlot Item;
        public int Slot;

        public void Setup(LoadoutSlot item, int slot)
        {
            Item = item;
            Slot = slot;
            if (item.IsEmpty)
            {
                _sprite.sprite = null;
                _sprite.gameObject.SetActive(false);
            }
            else
            {
                _sprite.sprite = item.Icon;
                _sprite.gameObject.SetActive(true);
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
    }
}