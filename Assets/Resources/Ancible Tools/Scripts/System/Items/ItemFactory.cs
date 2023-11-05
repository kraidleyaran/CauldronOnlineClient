using System.Collections.Generic;
using System.Linq;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class ItemFactory : MonoBehaviour
    {
        public static ItemLootController ItemLoot => _instance._itemLootTemplate;
        public static KeyItemLootController KeyItemLoot => _instance._keyItemLootTemplate;
        public static LootSpawnController LootSpawn => _instance._lootSpawnTemplate;

        public static BonusTag ExplosiveTag => _instance._explosive;

        private static ItemFactory _instance = null;

        [SerializeField] private string _internalItemPath = string.Empty;
        [SerializeField] private ItemLootController _itemLootTemplate;
        [SerializeField] private KeyItemLootController _keyItemLootTemplate;
        [SerializeField] private LootSpawnController _lootSpawnTemplate;
        [SerializeField] private string _tagsInternalPath = string.Empty;
        
        [Header("Item Quality Colors")]
        [SerializeField] private Color _normal = Color.white;
        [SerializeField] private Color _polished = Color.blue;
        [SerializeField] private Color _forged = Color.green;
        [SerializeField] private Color _rare = Color.blue;
        [SerializeField] private Color _ornate = Color.yellow;
        [SerializeField] private Color _legendary = Color.yellow;

        [Header("Tags")]
        [SerializeField] private BonusTag _explosive;

        private Dictionary<string, WorldItem> _items = new Dictionary<string, WorldItem>();
        private Dictionary<string, BonusTag> _tags = new Dictionary<string, BonusTag>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _items = UnityEngine.Resources.LoadAll<WorldItem>(_internalItemPath).ToDictionary(i => i.name, i => i);
            _tags = UnityEngine.Resources.LoadAll<BonusTag>(_tagsInternalPath).ToDictionary(t => t.name, t => t);
            Debug.Log($"Loaded {_items.Count} World Items");
            SubscribeToMessages();
        }

        public static WorldItem GetItemByName(string itemName)
        {
            if (_instance._items.TryGetValue(itemName, out var worldItem))
            {
                return worldItem;
            }

            return null;
        }

        public static Color GetQualityColor(WorldItem item)
        {
            switch (item.Quality)
            {
                case ItemQuality.Uncommon:
                    return _instance._polished;
                case ItemQuality.Forged:
                    return _instance._forged;
                case ItemQuality.Rare:
                    return _instance._rare;
                case ItemQuality.Ornate:
                    return _instance._ornate;
                case ItemQuality.Legendary:
                    return _instance._legendary;
                default:
                    return _instance._normal;
            }
        }

        public static WorldItem[] GetAllItems()
        {
            return _instance._items.Values.ToArray();
        }

        public static BonusTag GetTag(string tagName)
        {
            if (_instance._tags.TryGetValue(tagName, out var tag))
            {
                return tag;
            }

            return null;
        }

        public static BonusTag[] GetTags(string[] tags)
        {
            var returnTags = new List<BonusTag>();
            foreach (var tagName in tags)
            {
                if (_instance._tags.TryGetValue(tagName, out var tag))
                {
                    returnTags.Add(tag);
                }
            }

            return returnTags.ToArray();
        }
        

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClientItemRewardMessage>(ClientItemReward);
        }

        private void ClientItemReward(ClientItemRewardMessage msg)
        {
            if (DataController.WorldState != WorldState.Active)
            {
                var addItemMsg = MessageFactory.GenerateAddItemMsg();
                foreach (var itemData in msg.Items)
                {
                    var item = GetItemByName(itemData.Item);
                    if (item)
                    {
                        addItemMsg.Item = item;
                        addItemMsg.Stack = itemData.Stack;
                        _instance.gameObject.SendMessageTo(addItemMsg, ObjectManager.Player);
                        
                    }
                }
                MessageFactory.CacheMessage(addItemMsg);
            }
            else
            {
                foreach (var itemData in msg.Items)
                {
                    var item = GetItemByName(itemData.Item);
                    if (item)
                    {
                        var itemController = Instantiate(ItemLoot, ObjectManager.Player.transform.position.ToVector2(), Quaternion.identity);
                        itemController.Setup(item, itemData.Stack, StaticMethods.RandomDirection(), item.MaxStack > 1);
                        ObjectManager.RegisterObject(itemController.gameObject);
                    }
                }
                
                
            }
        }

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
                gameObject.UnsubscribeFromAllMessages();
            }
        }
    }
}