using CauldronOnlineCommon.Data.Quests;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Quests
{
    [CreateAssetMenu(fileName = "Quest Objective", menuName = "Ancible Tools/Server/Quests/Objectives/Default Objective")]
    public class QuestObjective : ScriptableObject
    {
        public int RequiredAmount;

        public virtual QuestObjectiveData GetData()
        {
            return new QuestObjectiveData {RequiredAmount = RequiredAmount};
        }
    }
}