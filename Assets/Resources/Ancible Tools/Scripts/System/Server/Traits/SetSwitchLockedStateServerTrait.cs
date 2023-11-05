using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Set Switch Locked State Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Switch/Set Switch Locked State")]
    public class SetSwitchLockedStateServerTrait : ServerTrait
    {
        [SerializeField] private bool _lock = false;

        public override WorldTraitData GetData()
        {
            return new SetSwitchLockStateTraitData {Lock = _lock, MaxStack = MaxStack, Name = name};
        }
    }
}