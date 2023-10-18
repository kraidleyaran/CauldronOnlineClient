using System;
using CauldronOnlineCommon.Data.Switches;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches
{
    [Serializable]
    public class RequiredSwitchSignal
    {
        public SwitchSignal Switch;
        public int Signal;

        public RequiredSwitchSignalData GetData()
        {
            return new RequiredSwitchSignalData {Switch = Switch.name, Signal = Signal};
        }
    }
}