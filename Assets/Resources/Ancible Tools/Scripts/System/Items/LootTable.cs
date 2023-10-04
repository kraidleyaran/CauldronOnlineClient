using System;
using System.Linq;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Loot Table", menuName = "Ancible Tools/Items/Loot Table")]
    public class LootTable : ScriptableObject
    {
        public LootRoll[] LootRolls = new LootRoll[0];

        public LootTableData GetData()
        {
            return new LootTableData
            {
                Name = name,
                LootRolls = LootRolls.Where(r => r != null).Select(r => r.GetData()).ToArray()
            };
        }
    }
}