using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Advance Switch Signal Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Switch/Advance Switch Signal")]
    public class AdvanceSwitchSignalServerTrait : ServerTrait
    {
        public override WorldTraitData GetData()
        {
            return new AdvanceSwitchSignalTraitData {Name = name, MaxStack = MaxStack};
        }
    }
}