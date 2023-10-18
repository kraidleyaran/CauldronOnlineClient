using System;
using CauldronOnlineCommon.Data.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class ActiveRecipe
    {
        public WorldItem Item;
        public WorldItemStackData[] Recipe;
        public int Stack;
        public int Cost;

        public ActiveRecipe(WorldItem item, WorldItemStackData[] recipe, int stack, int cost)
        {
            Item = item;
            Recipe = recipe;
            Stack = stack;
            Cost = cost;
        }
    }
}