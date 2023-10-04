using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Ai Movement Server Trait", menuName = "Ancible Tools/Server/Traits/Ai/Ai Movement")]
    public class AiMovementServerTrait : ServerTrait
    {
        [SerializeField] private int _moveSpeed = 1;

        public override WorldTraitData GetData()
        {
            return new AiMovementTraitData {MoveSpeed = _moveSpeed, Name = name, MaxStack = MaxStack};
        }
    }
}