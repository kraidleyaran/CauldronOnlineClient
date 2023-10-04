using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Ai State Server Trait", menuName = "Ancible Tools/Server/Traits/Ai/Ai State")]
    public class AiStateServerTrait : ServerTrait
    {
        public override WorldTraitData GetData()
        {
            return new AiStateTraitData {Name = name, MaxStack = MaxStack};
        }
    }
}