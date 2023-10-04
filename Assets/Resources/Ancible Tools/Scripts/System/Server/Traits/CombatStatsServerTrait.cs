using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Combat Stats Server Trait", menuName = "Ancible Tools/Server/Traits/Combat/Combat Stats")]
    public class CombatStatsServerTrait : ServerTrait
    {
        [SerializeField] private CombatStats _baseStats;

        public override WorldTraitData GetData()
        {
            return new CombatStatsTraitData {Stats = _baseStats, Name = name, MaxStack = MaxStack};
        }
    }
}