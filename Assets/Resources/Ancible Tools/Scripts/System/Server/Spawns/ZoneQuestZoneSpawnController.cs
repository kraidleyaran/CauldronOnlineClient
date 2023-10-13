using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Quests;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class ZoneQuestZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private QuestObjective[] _objectives = new QuestObjective[0];
        [SerializeField] private int _range = 0;
        [SerializeField] private ServerTrait[] _applyOnComplete;
        [SerializeField] private TriggerEvent[] _triggerEventsOnComplete;
        [SerializeField] private bool _resetQuest = true;
        [SerializeField] private WorldIntRange _resetTicks = new WorldIntRange(1,1);
        

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new ZoneQuestParameter
            {
                Name = name,
                Objectives = _objectives.Where(o => o).Select(o => o.GetData()).ToArray(),
                Range = _range,
                ApplyOnComplete = _applyOnComplete.Where(t => t).Select(t => t.name).ToArray(),
                TriggerEventOnComplete = _triggerEventsOnComplete.Where(t => t).Select(t => t.name).ToArray(),
                ResetQuest = _resetQuest,
                ResetTicks = _resetTicks
            });
            return data;
        }
    }
}