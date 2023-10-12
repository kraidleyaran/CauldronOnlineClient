using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class ResourceItemStack
    {
        public ResourceItem Item;
        public int Stack;

        public ItemStack ToItemStack()
        {
            return new ItemStack {Item = Item, Stack = Stack};
        }

        public string GetRequiredDescription()
        {
            return $"{Item.DisplayName} x{Stack}";
        }
    }

}