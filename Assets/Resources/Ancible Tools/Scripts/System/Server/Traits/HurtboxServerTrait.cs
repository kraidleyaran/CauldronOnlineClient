using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Hurtbox Server Trait", menuName = "Ancible Tools/Server/Traits/Combat/Hurtbox")]
    public class HurtboxServerTrait : ServerTrait
    {
        [SerializeField] private WorldVector2Int _size = WorldVector2Int.One;
        [SerializeField] private WorldVector2Int _offset = WorldVector2Int.Zero;
        [SerializeField] private bool _receiveKnockback = true;

        public override WorldTraitData GetData()
        {
            return new HurtboxTraitData {Size = _size, Offset = _offset, Knockback = _receiveKnockback, MaxStack = 1, Name = name};
        }
    }
}