using System.Linq;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Ai Aggro Server Trait", menuName = "Ancible Tools/Server/Traits/Ai/Ai Aggro")]
    public class AiAggroServerTrait : ServerTrait
    {
        [SerializeField] private int _aggroRange = 1;
        [SerializeField] private int _defaultAggro = 25;
        [SerializeField] private ServerTrait[] _applyOnAggro;
        [SerializeField] private float _diagonalCost = 0f;

        public override WorldTraitData GetData()
        {
            return new AiAggroTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                AggroRange = _aggroRange,
                DefaultAggro = _defaultAggro,
                ApplyOnAggro = _applyOnAggro.Where(t => t).Select(t => t.name).ToArray(),
                DiagonalCost = _diagonalCost
            };
        }
    }
}