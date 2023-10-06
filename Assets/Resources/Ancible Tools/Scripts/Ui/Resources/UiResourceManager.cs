using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Resources
{
    public class UiResourceManager : MonoBehaviour
    {
        [SerializeField] private UiResourceController _resourceTemplate;
        [SerializeField] private HorizontalLayoutGroup _grid;

        private Dictionary<ResourceItem, UiResourceController> _controllers = new Dictionary<ResourceItem, UiResourceController>();

        void Awake()
        {
            RefreshResources();
            SubscribeToMessages();
        }

        private bool IsResourceItem(ItemStack stack)
        {
            return stack.Item.Type == ItemType.Resource;
        }

        private void RefreshResources()
        {
            var queryItemsMsg = MessageFactory.GenerateQueryItemsMessage();
            queryItemsMsg.Query = IsResourceItem;
            queryItemsMsg.DoAfter = UpdateResources;
            gameObject.SendMessageTo(queryItemsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryItemsMsg);
        }

        private void UpdateResources(ItemStack[] items)
        {
            var resourcesStacks = new Dictionary<ResourceItem, int>();
            foreach (var item in items)
            {
                if (item.Item is ResourceItem resource)
                {
                    if (!resourcesStacks.ContainsKey(resource))
                    {
                        resourcesStacks.Add(resource, 0);
                    }

                    resourcesStacks[resource] += item.Stack;
                }
            }

            foreach (var resource in resourcesStacks)
            {
                if (!_controllers.TryGetValue(resource.Key, out var controller))
                {
                    controller = Instantiate(_resourceTemplate, _grid.transform);
                    _controllers.Add(resource.Key, controller);
                }
                controller.Setup(resource.Key, resource.Value);
            }
            var ordered = _controllers.Values.OrderBy(c => c.Item.DisplayName).ToArray();
            for (var i = 0; i < ordered.Length; i++)
            {
                ordered[i].transform.SetSiblingIndex(i);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerInventoryUpdatedMessage>(PlayerInventoryUpdated);
        }

        private void PlayerInventoryUpdated(PlayerInventoryUpdatedMessage msg)
        {
            RefreshResources();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}