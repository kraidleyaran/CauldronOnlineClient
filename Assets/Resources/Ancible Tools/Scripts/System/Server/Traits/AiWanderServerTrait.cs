using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Ai Wander Server Trait", menuName = "Ancible Tools/Server/Traits/Ai/Ai Wander")]
    public class AiWanderServerTrait : ServerTrait
    {
        [SerializeField] private int _wanderRange;
        [SerializeField] private bool _anchor = false;
        [SerializeField] private WorldIntRange _idleTicks;
        [SerializeField] [Range(0f, 1f)] private float _chanceToIdle = 0f;
        [SerializeField] private float _diagonalCost = 0f;

        public override WorldTraitData GetData()
        {
            return new AiWanderTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Anchor = _anchor,
                WanderRange = _wanderRange,
                IdleTicks = _idleTicks,
                ChanceToIdle = _chanceToIdle,
                DiagonalCost =  _diagonalCost
            };
        }
    }
}