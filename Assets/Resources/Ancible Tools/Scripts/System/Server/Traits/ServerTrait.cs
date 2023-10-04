using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    public class ServerTrait : ScriptableObject
    {
        public int MaxStack = 1;

        public virtual WorldTraitData GetData()
        {
            return new WorldTraitData {Name = name, MaxStack = MaxStack};
        }
    }
}