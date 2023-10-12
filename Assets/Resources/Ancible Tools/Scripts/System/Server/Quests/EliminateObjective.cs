using Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates;
using CauldronOnlineCommon.Data.Quests;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Quests
{
    [CreateAssetMenu(fileName = "Eliminate Objective", menuName = "Ancible Tools/Server/Quests/Objectives/Eliminate")]
    public class EliminateObjective : QuestObjective
    {
        [SerializeField] private ZoneSpawn _template;

        public override QuestObjectiveData GetData()
        {
            return new EliminateQuestObjectiveData {Template = _template.GetData(), RequiredAmount = RequiredAmount};
        }
    }
}