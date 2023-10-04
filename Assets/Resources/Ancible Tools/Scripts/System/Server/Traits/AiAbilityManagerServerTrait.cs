using System.Linq;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Ai Ability Manager Server Trait", menuName = "Ancible Tools/Server/Traits/Ai/Ai Ability Manager")]
    public class AiAbilityManagerServerTrait : ServerTrait
    {
        public const int FRAMES_PER_WORLD_TICK = 10;

        [SerializeField] private AiAbility[] _abilities = new AiAbility[0];

        public override WorldTraitData GetData()
        {
            return new AiAbilityManagerTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Abilities = _abilities.Where(a => a.Ability).Select(a => a.GetData(FRAMES_PER_WORLD_TICK)).ToArray()
            };
        }
    }
}