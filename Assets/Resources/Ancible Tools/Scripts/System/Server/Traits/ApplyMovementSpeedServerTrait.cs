using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Apply Movement Speed Server Trait", menuName = "Ancible Tools/Server/Traits/Combat/Apply Movement Speed")]
    public class ApplyMovementSpeedServerTrait : ServerTrait
    {
        [SerializeField] private int _amount;
        [SerializeField] private bool _bonus = true;

        public override WorldTraitData GetData()
        {
            return new ApplyMovementSpeedTraitData {Name = name, MaxStack = MaxStack, Amount = _amount, Bonus = _bonus};
        }
    }
}