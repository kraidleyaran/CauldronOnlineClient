using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches
{
    [Serializable]
    public class OnSwitchSignal
    {
        public SwitchSignal Switch;
        public int RequiredSignal;
        public ServerTrait[] ApplyOnSignal;
        

        public OnSwitchSignalData GetData()
        {
            return new OnSwitchSignalData
            {
                Signal = RequiredSignal,
                ApplyOnSignal = ApplyOnSignal.Where(t => t).Select(t => t.name).ToArray(),
                Switch = Switch.name 
            };
        }
    }
}