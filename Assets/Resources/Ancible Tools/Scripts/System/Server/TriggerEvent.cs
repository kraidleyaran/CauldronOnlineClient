using CauldronOnlineCommon.Data.TriggerEvents;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server
{
    [CreateAssetMenu(fileName = "Trigger Event", menuName = "Ancible Tools/Server/Trigger Event")]
    public class TriggerEvent : ScriptableObject
    {
        public int MaxActivations = 0;

        public TriggerEventData GetData()
        {
            return new TriggerEventData {Name = name, MaxActivations = MaxActivations};
        }
    }
}