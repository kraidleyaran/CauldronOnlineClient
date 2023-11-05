using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Walled Server Trait", menuName = "Ancible Tools/Server/Traits/General/Walled")]
    public class WalledServerTrait : ServerTrait
    {
        [SerializeField] private ServerHitbox _hitbox;
        [SerializeField] private bool _ignoreGround = false;
        [SerializeField] private bool _checkForPlayer = false;

        public override WorldTraitData GetData()
        {
            return new WalledTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Hitbox = _hitbox.GetData(),
                IgnoreGround = _ignoreGround,
                CheckForPlayer = _checkForPlayer
            };
        }
    }
}