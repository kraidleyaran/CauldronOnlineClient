using Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    public class MultiSwitchSignalReceiverServerTrait : ServerTrait
    {
        [SerializeField] private OnMultiSwitchSignal _requirement;

        public override WorldTraitData GetData()
        {
            return new MultiSwitchSignalReceiverTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Data = _requirement.GetData()
            };
        }
    }
}