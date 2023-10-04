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

        public ShopItemData GetData()
        {
            return new ShopItemData {Item = Item.name, Stack = Stack, Cost = Cost};
        }

        public void Destroy()
        {
            Item = null;
            Stack = 0;
            Cost = 0;
        }
    }
}