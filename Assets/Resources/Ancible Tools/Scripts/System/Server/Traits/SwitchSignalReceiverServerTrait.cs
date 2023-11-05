using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Switch Signal Receiver Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Switch/Switch Signal Receiver")]
    public class SwitchSignalReceiverServerTrait : ServerTrait
    {
        [SerializeField] private OnSwitchSignal[] _onSwitchSignal;

        public override WorldTraitData GetData()
        {
            return new SwitchSignalReceiverTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                OnSignals = _onSwitchSignal.Select(s => s.GetData()).ToArray()
            };
        }
    }
}