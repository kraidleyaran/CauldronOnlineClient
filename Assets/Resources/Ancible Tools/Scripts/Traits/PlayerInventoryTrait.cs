using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Inventory Trait", menuName = "Ancible Tools/Traits/Player/Player Inventory")]
    public class PlayerInventoryTrait : Trait
    {
        [SerializeField] private ItemStack[] _startingItems = new ItemStack[0];

        private List<ItemStack> _items = new List<ItemStack>();
        private int _gold = 0;

        private FillLoadoutStackMessage _fillLoadoutStackMsg = new FillLoadoutStackMessage();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            foreach (var item in _startingItems)
            {
                if (item.Stack > 0)
                {
                    AddItem(item.Item, item.Stack);
                }
            }
            SubscribeToMessages();
        }

        private void AddItem(WorldItem item, int stack)
        {
            if (item.Type == ItemType.Instant)
            {
                if (item is InstantItem instant)
                {
                    for (var i = 0; i < stack; i++)
                    {
                        instant.Apply(_controller.transform.parent.gameObject);
                    }
                }
            }
            else if (item.Type == ItemType.Key)
            {
                ClientController.SendToServer(new ClientAddKeyItemMessage{Item = item.name, Stack = stack});
            }
            else
            {
                var remainingStack = stack;
                if (item.Type == ItemType.Action && item is ActionItem actionItem)
                {
                    _fillLoadoutStackMsg.Item = actionItem;
                    _fillLoadoutStackMsg.Stack = remainingStack;
                    _fillLoadoutStackMsg.DoAfter = remainder => remainingStack = remainder;
                    _controller.gameObject.SendMessageTo(_fillLoadoutStackMsg, _controller.transform.parent.gameObject);
                }

                if (remainingStack > 0)
                {
                    var availableStacks = _items.Where(i => i.Item == item && i.Stack < item.MaxStack).OrderByDescending(i => i.Stack).ToArray();
                    foreach (var itemStack in availableStacks)
                    {
                        if (itemStack.Stack + remainingStack < item.MaxStack)
                        {
                            itemStack.Stack += remainingStack;
                            remainingStack = 0;
                            break;
                        }
                        else
                        {
                            var add = item.MaxStack - itemStack.Stack;
                            itemStack.Stack = item.MaxStack;
                            remainingStack -= add;
                        }
                    }

                    if (remainingStack > 0)
                    {
                        while (remainingStack > 0)
                        {
                            var addStack = remainingStack > item.MaxStack ? item.MaxStack : remainingStack;
                            _items.Add(new ItemStack { Item = item, Stack = addStack });
                            remainingStack -= addStack;
                        }
                    }
                }
                
            }
            
        }

        private void RemoveItem(WorldItem item, int stack)
        {
            var availableStacks = _items.Where(i => i.Item == item).OrderBy(i => i.Stack);
            var remainingStack = stack;
            foreach (var itemStack in availableStacks)
            {
                if (remainingStack < itemStack.Stack)
                {
                    itemStack.Stack -= remainingStack;
                    break;
                }
                else
                {
                    remainingStack -= itemStack.Stack;
                    _items.Remove(itemStack);
                }
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddItemMessage>(AddItem, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveItemMessage>(RemoveItem, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryInventoryMessage>(QueryInventory, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddGoldMessage>(AddGold, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveGoldMessage>(RemoveGold, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryGoldMessage>(QueryGold, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryItemsMessage>(QueryItems, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<HasItemsQueryMessage>(HasItemsQuery, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryShopMessage>(QueryShop, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetInventoryMessage>(SetInventory, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAvailableResourceUsesMessage>(QueryAvailableResourceUses, _instanceId);
        }

        private void AddItem(AddItemMessage msg)
        {
            AddItem(msg.Item, msg.Stack);
            _controller.gameObject.SendMessage(PlayerInventoryUpdatedMessage.INSTANCE);
        }

        private void RemoveItem(RemoveItemMessage msg)
        {
            RemoveItem(msg.Item, msg.Stack);
            if (msg.Update)
            {
                _controller.gameObject.SendMessage(PlayerInventoryUpdatedMessage.INSTANCE);
            }
            
        }

        private void QueryInventory(QueryInventoryMessage msg)
        {
            msg.DoAfter.Invoke(_items.ToArray());
        }

        private void AddGold(AddGoldMessage msg)
        {
            _gold += msg.Amount;
            _controller.gameObject.SendMessage(PlayerGoldUpdatedMessage.INSTANCE);
        }


        private void RemoveGold(RemoveGoldMessage msg)
        {
            _gold -= msg.Amount;
            _controller.gameObject.SendMessage(PlayerGoldUpdatedMessage.INSTANCE);
        }

        private void QueryGold(QueryGoldMessage msg)
        {
            msg.DoAfter.Invoke(_gold);
        }

        private void HasItemsQuery(HasItemsQueryMessage msg)
        {
            var hasItems = true;
            foreach (var stack in msg.Items)
            {
                var available = msg.Items.Where(i => i.Item == stack.Item).ToArray();
                if (available.Length > 0)
                {
                    var amount = available.Sum(a => a.Stack);
                    if (amount < stack.Stack)
                    {
                        hasItems = false;
                        break;
                    }
                }
                else
                {
                    hasItems = false;
                    break;
                }
            }
            msg.DoAfter.Invoke(hasItems);
        }

        private void QueryItems(QueryItemsMessage msg)
        {
            msg.DoAfter.Invoke(_items.Where(msg.Query.Invoke).ToArray());
        }

        private void QueryShop(QueryShopMessage msg)
        {
            var items = _items.Where(i => i.Item.SellValue > 0).Select(i => i.ToShopItem()).ToArray();
            msg.DoAfter.Invoke(items);
        }

        private void SetInventory(SetInventoryMessage msg)
        {
            _gold = msg.Gold;
            _items.Clear();
            foreach (var itemData in msg.Items)
            {
                var item = ItemFactory.GetItemByName(itemData.Item);
                if (item)
                {
                    AddItem(item, itemData.Stack);
                }
            }
        }

        private void QueryAvailableResourceUses(QueryAvailableResourceUsesMessage msg)
        {
            var requiredItems = msg.Items.ToArray();
            var availableUses = -1;
            foreach (var required in requiredItems)
            {
                var totalStack = _items.Where(i => i.Item == required.Item).Sum(i => i.Stack);
                if (totalStack > 0)
                {
                    var uses = totalStack / required.Stack;
                    if (uses <= 0)
                    {
                        availableUses = 0;
                        break;
                    }
                }
                else
                {
                    availableUses = 0;
                    break;
                }
            }

            if (availableUses < 0)
            {
                availableUses = 0;
            }
            msg.DoAfter.Invoke(availableUses);
        }
    }
}