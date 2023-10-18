using System;
using CauldronOnlineCommon.Data.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class ShopItem
    {
        public WorldItem Item;
        public int Stack;
        public int Cost;

        public ShopItem()
        {

        }

        public ShopItem(WorldItem item, ShopItemData data)
        {
            Item = item;
            Stack = data.Stack;
            Cost = data.Cost;
        }

        public virtual ShopItemData GetData()
        {
            return new ShopItemData {Item = Item.name, Stack = Stack, Cost = Cost};
        }

        public virtual void Destroy()
        {
            Item = null;
            Stack = 0;
            Cost = 0;
        }
    }
}