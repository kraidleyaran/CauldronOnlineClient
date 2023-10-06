using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Charge Server Trait", menuName = "Ancible Tools/Server/Traits/Combat/Charge")]
    public class ChargeServerTrait : ServerTrait
    {
        [SerializeField] private int _speed = 1;
        [SerializeField] private int _distance = 1;

        public override WorldTraitData GetData()
        {
            return new ChargeTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Speed = _speed,
                Distance = _distance
            };
        }
    }
}