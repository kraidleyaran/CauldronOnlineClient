using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data.Switches;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches
{
    [Serializable]
    public class OnSwitchSignal
    {
        public SwitchSignal Switch;
        public int RequiredSignal;
        public ServerTrait[] ApplyOnSignal;
        public TriggerEvent[] ApplyEventOnSignal = new TriggerEvent[0];

        public OnSwitchSignalData GetData()
        {
            return new OnSwitchSignalData
            {
                Signal = RequiredSignal,
                ApplyOnSignal = ApplyOnSignal.Where(t => t).Select(t => t.name).ToArray(),
                ApplyEventsOnSignal = ApplyEventOnSignal.Where(t => t).Select(t => t.name).ToArray(),
                Switch = Switch.name
            };
        }
    }
}