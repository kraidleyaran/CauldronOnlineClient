using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Loot Server Trait", menuName = "Ancible Tools/Server/Traits/Items/Loot")]
    public class LootServerTrait : ServerTrait
    {
        [SerializeField] private LootTable _lootTable;
        [SerializeField] private WorldIntRange _drops = new WorldIntRange(0,1);

        public override WorldTraitData GetData()
        {
            return new LootTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                LootTable = _lootTable.name,
                Drops = _drops,
            };
        }
    }
}