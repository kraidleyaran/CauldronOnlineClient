using System.Linq;
using CauldronOnlineCommon.Data.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Loot Table", menuName = "Ancible Tools/Items/Loot/Extended Loot Table")]
    public class ExtendedLootTable : LootTable
    {
        [SerializeField] private LootTable _baseTable;

        public override LootTableData GetData()
        {
            var loot = _baseTable.LootRolls.ToList();
            loot.AddRange(LootRolls);
            return new LootTableData {Name = name, LootRolls = loot.Select(l => l.GetData()).ToArray()};
        }
    }
}