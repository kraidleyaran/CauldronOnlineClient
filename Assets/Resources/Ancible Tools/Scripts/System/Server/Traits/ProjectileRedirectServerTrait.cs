using System.Linq;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Projectile Redirect Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Projectile Redirect")]
    public class ProjectileRedirectServerTrait : ServerTrait
    {
        [SerializeField] private WorldVector2Int _direction = WorldVector2Int.Down;
        [SerializeField] private ServerHitbox _hitbox;
        [SerializeField] private BonusTag[] _tags = new BonusTag[0];

        public override WorldTraitData GetData()
        {
            return new ProjectileRedirectTraitData { Name = name, MaxStack = MaxStack, Direction = _direction, Hitbox = _hitbox.GetData(), Tags = _tags.Where(t => t).Select(t => t.name).ToArray()};
        }
    }
}