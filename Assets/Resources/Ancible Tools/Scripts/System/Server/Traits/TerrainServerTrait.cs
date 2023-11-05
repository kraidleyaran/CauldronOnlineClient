using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Terrain Server Trait", menuName = "Ancible Tools/Server/Traits/General/Terrain")]
    public class TerrainServerTrait : ServerTrait
    {
        [SerializeField] private ServerHitbox _hitbox;
        [SerializeField] private bool _ground = false;

        public override WorldTraitData GetData()
        {
            return new TerrainTraitData {Name = name, MaxStack = MaxStack, Hitbox = _hitbox.GetData(), IsGround = _ground};
        }
    }
}