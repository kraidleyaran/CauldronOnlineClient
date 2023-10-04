using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class ItemFactory : MonoBehaviour
    {
        public static ItemLootController ItemLoot => _instance._itemLootTemplate;

        private static ItemFactory _instance = null;

        [SerializeField] private string _internalItemPath = string.Empty;
        [SerializeField] private ItemLootController _itemLootTemplate;

        private Dictionary<string, WorldItem> _items = new Dictionary<string, WorldItem>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _items = UnityEngine.Resources.LoadAll<WorldItem>(_internalItemPath).ToDictionary(i => i.name, i => i);
            Debug.Log($"Loaded {_items.Count} World Items");
        }

        public static WorldItem GetItemByName(string itemName)
        {
            if (_instance._items.TryGetValue(itemName, out var worldItem))
            {
                return worldItem;
            }

            return null;
        }

        
    }
}