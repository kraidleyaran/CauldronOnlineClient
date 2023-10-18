using System.Linq;
using CauldronOnlineCommon.Data.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Item Recipe", menuName = "Ancible Tools/Items/Item Recipe")]
    public class ItemRecipe : ScriptableObject
    {
        public WorldItem Item;
        public int Stack = 1;
        public ItemIngredient[] Ingredients;
        public int Cost = 0;

        public ItemRecipeData GetData()
        {
            return new ItemRecipeData
            {
                Item = Item.name,
                Stack = Stack,
                Recipe = Ingredients.Where(i => i.Item).Select(i => i.GetData()).ToArray(),
                Gold = Cost
            };
        }
    }
}