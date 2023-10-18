using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Server;
using CauldronOnlineCommon.Data.Items;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class RestrictedShopItem : ShopItem
    {
        public TriggerEvent TriggerEvent = null;

        public override ShopItemData GetData()
        {
            return new RestrictedShopItemData {Item = Item.name, Stack = Stack, Cost = Cost, TriggerEvent = TriggerEvent.name};
        }

        public override void Destroy()
        {
            base.Destroy();
            TriggerEvent = null;
        }
    }
}