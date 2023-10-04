using System.Linq;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Hitbox Server Trait", menuName = "Ancible Tools/Server/Traits/Combat/Hitbox")]
    public class HitboxServerTrait : ServerTrait
    {
        [SerializeField] private ApplyServerHitbox[] _hitboxes = new ApplyServerHitbox[0];

        public override WorldTraitData GetData()
        {
            return new HitboxTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Hitboxes = _hitboxes.Select(h => h.GetApplyData()).ToArray()
            };
        }
    }
}