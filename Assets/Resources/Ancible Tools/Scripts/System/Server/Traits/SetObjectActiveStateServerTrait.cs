using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Set Object Active State Server Trait", menuName = "Ancible Tools/Server/Traits/Object State/Set Object Active State")]
    public class SetObjectActiveStateServerTrait : ServerTrait
    {
        [SerializeField] private bool _active = false;

        public override WorldTraitData GetData()
        {
            return new SetObjectActiveStateTraitData {Active = _active, Name = name, MaxStack = MaxStack};
        }
    }
}