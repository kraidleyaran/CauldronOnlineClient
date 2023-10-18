using System.Linq;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Trigger Event Receiver Server Trait", menuName = "Ancible Tools/Server/Traits/Trigger Events/Trigger Event Receiver")]
    public class TriggerEventReceiverServerTrait : ServerTrait
    {
        [SerializeField] private TriggerEvent[] _triggerEvents;
        [SerializeField] private ServerTrait[] _applyOnEvent;
        [SerializeField] private bool _requireAllEvents = false;

        public override WorldTraitData GetData()
        {
            return new TriggerEventReceiverTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                ApplyOnTriggerEvent = _applyOnEvent.Where(t => t).Select(t => t.name).ToArray(),
                TriggerEvents = _triggerEvents.Where(t => t).Select(t => t.name).ToArray(),
                RequireAllEvents = _requireAllEvents
            };
        }
    }
}