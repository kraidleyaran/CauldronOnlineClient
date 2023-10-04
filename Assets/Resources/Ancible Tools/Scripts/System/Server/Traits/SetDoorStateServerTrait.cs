using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Set Door State Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Set Door State")]
    public class SetDoorStateServerTrait : ServerTrait
    {
        [SerializeField] private bool _open = false;

        public override WorldTraitData GetData()
        {
            return new SetDoorStateTraitData {MaxStack = MaxStack, Name = name, Open = _open};
        }
    }
}