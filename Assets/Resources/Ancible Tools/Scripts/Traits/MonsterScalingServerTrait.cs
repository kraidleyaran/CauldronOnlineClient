using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Monster Scaling Server Trait", menuName = "Ancible Tools/Server/Traits/Monster/Monster Scaling")]
    public class MonsterScalingServerTrait : ServerTrait
    {
        [SerializeField] private ServerTrait[] _applyPerPlayer = new ServerTrait[0];
        [SerializeField] private CombatStats _stats = new CombatStats();

        public override WorldTraitData GetData()
        {
            return new MonsterScalingTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                ApplyPerPlayer = _applyPerPlayer.Where(t => t).Select(t => t.name).ToArray(),
                Stats = _stats
            };
        }
    }
}