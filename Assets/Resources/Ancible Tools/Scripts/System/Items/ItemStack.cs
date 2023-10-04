using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Items;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class ItemStack : WorldStack<WorldItem>
    {
        public ShopItem ToShopItem()
        {
            return new ShopItem{Item = Item, Stack = Stack, Cost = Stack * Item.SellValue};
        }

        public ItemStackData GetData()
        {
            return new ItemStackData {Item = Item.name, Stack = Stack};
        }

        public WorldItemStackData GetWorldData()
        {
            return new WorldItemStackData {Item = Item.name, Stack = Stack};
        }
    }
}