using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Dev
{
    public class UiDevGiveItemController : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Dropdown _itemDropdown;
        [SerializeField] private InputField _stackInput;

        private WorldItem[] _items = new WorldItem[0];

        void Awake()
        {
            _items = ItemFactory.GetAllItems().OrderBy(i => i.DisplayName).ThenBy(i => i.Quality).ToArray();
            _itemDropdown.ClearOptions();
            _itemDropdown.AddOptions(_items.Select(i => i.GetDisplayName()).ToList());
            _itemDropdown.SetValueWithoutNotify(0);
            UpdateSelected();
        }

        public void GiveItem()
        {
            if (int.TryParse(_stackInput.text, out var stack))
            {
                var item = _items[_itemDropdown.value];
                var addItemMsg = MessageFactory.GenerateAddItemMsg();
                addItemMsg.Item = item;
                addItemMsg.Stack = stack;
                gameObject.SendMessageTo(addItemMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(addItemMsg);
            }

            
        }

        public void DropdownUpdated()
        {
            UpdateSelected();
        }

        private void UpdateSelected()
        {
            var item = _items[_itemDropdown.value];
            _itemIcon.sprite = item.Sprite.Sprite;
        }
        
    }
}