using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Shop Trait", menuName = "Ancible Tools/Traits/Network/Network Shop")]
    public class NetworkShopTrait : InteractableTrait
    {
        private ShopItem[] _shopItems = new ShopItem[0];

        private bool _shopWindowOpen = false;

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupShopMessage>(SetupShop, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryShopMessage>(QueryShop, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ShopWindowClosedMessage>(ShopWindowClosed, _instanceId);
        }

        protected internal override void Interact()
        {
            if (!_shopWindowOpen)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                _shopWindowOpen = true;

                _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                {
                    var shopWindow = UiWindowManager.OpenWindow(UiController.Shop);
                    shopWindow.Setup(_controller.transform.parent.gameObject);
                }));

            }

        }

        private void SetupShop(SetupShopMessage msg)
        {
            var shopItems = new List<ShopItem>();
            foreach (var shopItem in msg.Items)
            {
                var item = ItemFactory.GetItemByName(shopItem.Item);
                if (item)
                {
                    shopItems.Add(new ShopItem(item, shopItem));
                }
            }

            _shopItems = shopItems.ToArray();
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector().ToVector2(false));
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }


        private void QueryShop(QueryShopMessage msg)
        {
            msg.DoAfter.Invoke(_shopItems);
        }

        private void ShopWindowClosed(ShopWindowClosedMessage msg)
        {
            _shopWindowOpen = false;
            _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Active;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }));
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
            }

            _hitboxController = null;
            foreach (var item in _shopItems)
            {
                item.Destroy();
            }
            _shopItems = new ShopItem[0];
            base.Destroy();
        }
    }
}