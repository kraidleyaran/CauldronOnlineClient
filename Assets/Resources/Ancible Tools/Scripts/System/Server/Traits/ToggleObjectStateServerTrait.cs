using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Toggle Object State Server Trait", menuName = "Ancible Tools/Server/Traits/Object State/Toggle Object State")]
    public class ToggleObjectStateServerTrait : ServerTrait
    {
        public override WorldTraitData GetData()
        {
            return new ToggleObjectStateTraitData {Name = name, MaxStack = MaxStack};
        }
    }
}