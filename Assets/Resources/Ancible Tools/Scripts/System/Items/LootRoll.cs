using System;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class LootRoll
    {
        public WorldItem Item;
        [Range(0f, 1f)] public float ChanceToDrop;
        public WorldIntRange Stack = new WorldIntRange(0, 1);
        public bool SpawnEachStack = false;

        public LootRollData GetData()
        {
            return new LootRollData {Item = Item.name, ChanceToDrop = ChanceToDrop, Stack = Stack, SpawnEachStack = SpawnEachStack};
        }
    }
}