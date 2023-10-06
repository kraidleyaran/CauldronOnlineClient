using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data.Combat;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server
{
    [Serializable]
    public class AiAbility
    {
        public WorldAbility Ability;
        public int Range;
        public int Priority;
        public AbilitySight Sight = AbilitySight.Any;
        public ServerTrait[] ApplyAfterCastOnServer = new ServerTrait[0];

        public AiAbilityData GetData(int framesPerWorldTick)
        {
            return new AiAbilityData
            {
                Ability = Ability.name,
                Range = Range,
                Cooldown = Ability.Cooldown,
                Priority = Priority,
                Length = Ability.GetWorldTicks(framesPerWorldTick),
                Sight =  Sight,
                ApplyAfterCast = ApplyAfterCastOnServer.Where(t => t).Select(t => t.name).ToArray()
            };
        }
    }
}