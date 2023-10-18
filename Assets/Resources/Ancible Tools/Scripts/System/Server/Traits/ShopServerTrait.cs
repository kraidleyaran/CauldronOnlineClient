using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Shop Server Trait", menuName = "Ancible Tools/Server/Traits/Items/Shop")]
    public class ShopServerTrait : ServerTrait
    {
        [SerializeField] private ShopItem[] _shopItems = new ShopItem[0];
        [SerializeField] private RestrictedShopItem[] _restrictedItems = new RestrictedShopItem[0];
        [SerializeField] private ServerHitbox _serverHitbox;

        public override WorldTraitData GetData()
        {
            return new ShopTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Items = _shopItems.Where(s => s.Item).Select(s => s.GetData()).ToArray(),
                Restricted = _restrictedItems.Where(r => r.Item).Select(r => r.GetData() as RestrictedShopItemData).ToArray(),
                Hitbox = _serverHitbox != null ? _serverHitbox.GetData() : new HitboxData { Size = WorldVector2Int.One}
            };
        }
    }
}