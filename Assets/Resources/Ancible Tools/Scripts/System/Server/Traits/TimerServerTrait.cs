using System.Linq;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Timer Server Trait", menuName = "Ancible Tools/Server/Traits/General/Timer")]
    public class TimerServerTrait : ServerTrait
    {
        [SerializeField] private ServerTrait[] _applyOnStart;
        [SerializeField] private ServerTrait[] _applyOnLoop;
        [SerializeField] private int _ticks;
        [SerializeField] private int _loops;
        [SerializeField] private bool _showOnClient = false;

        public override WorldTraitData GetData()
        {
            return new TimerTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                ApplyOnStart = _applyOnStart.Where(t => t).Select(t => t.name).ToArray(),
                ApplyOnLoop = _applyOnLoop.Where(t => t).Select(t => t.name).ToArray(),
                TotalTicks = _ticks,
                TotalLoops = _loops,
                ShowOnClient = _showOnClient
            };
        }
    }
}