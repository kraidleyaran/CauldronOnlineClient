using System;
using CauldronOnlineCommon.Data.Items;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class ItemIngredient
    {
        public WorldItem Item;
        public int Stack = 1;

        public WorldItemStackData GetData()
        {
            return new WorldItemStackData {Item = Item.name, Stack = Stack};
        }
    }
}