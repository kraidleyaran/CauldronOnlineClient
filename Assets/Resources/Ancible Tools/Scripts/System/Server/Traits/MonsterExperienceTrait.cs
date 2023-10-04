using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Monster Experience Server Trait", menuName = "Ancible Tools/Server/Traits/Combat/Monster Experience")]
    public class MonsterExperienceTrait : ServerTrait
    {
        [SerializeField] private WorldIntRange _experience = new WorldIntRange();

        public override WorldTraitData GetData()
        {
            return new MonsterExperienceTraitData {Experience = _experience, Name = name, MaxStack = MaxStack};
        }
    }
}