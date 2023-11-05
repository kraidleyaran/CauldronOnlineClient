using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Shop
{
    public class UiShopWindow : UiWindowBase
    {
        private static UiShopWindow _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private UiShopInventoryController _playerShop;
        [SerializeField] private UiShopInventoryController _npcShop;
        [SerializeField] private UiSellStackWindow _sellStackTemplate;
        [SerializeField] private GameObject _buy;
        [SerializeField] private GameObject _sell;

        private UiSellStackWindow _sellStackWindow = null;

        private GameObject _shopOwner = null;

        void Awake()
        {
            _instance = this;
            SubscribeToMessages();
        }

        public void Setup(GameObject shopOwner)
        {
            _shopOwner = shopOwner;
            _npcShop.Setup(_shopOwner);
            _playerShop.Setup(ObjectManager.Player);
            _playerShop.SetActive(false);
            _npcShop.SetActive(true);
            _buy.gameObject.SetActive(true);
            _sell.gameObject.SetActive(false);
        }

        public static void SetActiveShop(ShopInventoryType type, int cursorY)
        {
            switch (type)
            {
                case ShopInventoryType.Player:
                    _instance._npcShop.SetActive(false);
                    _instance._buy.gameObject.SetActive(false);
                    _instance._sell.gameObject.SetActive(true);
                    _instance.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                    {
                        _instance._playerShop.SetActive(true, cursorY);
                    }));
                    break;
                case ShopInventoryType.Shop:
                    _instance._playerShop.SetActive(false);
                    _instance._buy.gameObject.SetActive(true);
                    _instance._sell.gameObject.SetActive(false);
                    _instance.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                    {
                        _instance._npcShop.SetActive(true, cursorY);
                    }));
                    break;
            }
        }

        public static UiSellStackWindow ShowSellStack(ShopItem shopItem, Action<WorldItem, int, bool> doAfter)
        {
            _instance._sellStackWindow = UiWindowManager.OpenWindow(_instance._sellStackTemplate);
            _instance._sellStackWindow.Setup(shopItem, doAfter);
            return _instance._sellStackWindow;
        }

        public static void CloseSellStack()
        {
            if (_instance._sellStackWindow)
            {
                UiWindowManager.CloseWindow(_instance._sellStackWindow);
                _instance._sellStackWindow = null;
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!_instance._sellStackWindow)
            {
                if (!msg.Previous.Red && msg.Current.Red || !msg.Previous.PlayerMenu && msg.Current.PlayerMenu)
                {
                    UiWindowManager.CloseWindow(this);
                }
            }
        }

        public override void Close()
        {
            if (_instance._sellStackWindow)
            {
                UiWindowManager.CloseWindow(_instance._sellStackWindow);
                _instance._sellStackWindow = null;
            }
            gameObject.SendMessageTo(ShopWindowClosedMessage.INSTANCE, _shopOwner);
            _instance = null;
            base.Close();
        }
    }
}