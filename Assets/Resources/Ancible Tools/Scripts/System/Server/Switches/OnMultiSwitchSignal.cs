using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data.Switches;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches
{
    [Serializable]
    public class OnMultiSwitchSignal
    {
        public RequiredSwitchSignal[] RequiredSignals = new RequiredSwitchSignal[0];
        public ServerTrait[] ApplyOnSignal = new ServerTrait[0];
        public TriggerEvent[] ApplyTriggerEventOnSignal = new TriggerEvent[0];

        public OnMultiSwitchSignalData GetData()
        {
            return new OnMultiSwitchSignalData
            {
                RequiredSignals = RequiredSignals.Where(r => r.Switch).Select(r => r.GetData()).ToArray(),
                ApplyOnSignal = ApplyOnSignal.Where(t => t).Select(t => t.name).ToArray(),
                ApplyTriggerEventsOnSignal = ApplyTriggerEventOnSignal.Where(t => t).Select(t => t.name).ToArray()
            };
        }
    }
}