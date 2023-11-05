using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Set Switch Signal Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Switch/Set Switch Signal")]
    public class SetSwitchSignalServerTrait : ServerTrait
    {
        [SerializeField] private int _signal = 0;

        public override WorldTraitData GetData()
        {
            return new SetSwitchSignalTraitData {Signal = _signal, Name = name, MaxStack = MaxStack};
        }
    }
}