using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Shop;
using CauldronOnlineCommon;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiController : MonoBehaviour
    {
        public static UiShopWindow Shop => _instance._shopWindow;

        private static UiController _instance = null;

        [SerializeField] private UiWindowBase[] _disconnectedWorldWindows = new UiWindowBase[0];
        [SerializeField] private UiWindowBase[] _activeWorldWindows = new UiWindowBase[0];
        [SerializeField] private UiPlayerMenuManagerWindow _playerMenuWindow = null;
        [SerializeField] private UiShopWindow _shopWindow;
        [SerializeField] private UiDialogueWindow _dialogueWindow;

        private UiPlayerMenuManagerWindow _openPlayerMenuWindow = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        void Start()
        {
            foreach (var window in _disconnectedWorldWindows)
            {
                UiWindowManager.OpenWindow(window);
            }
        }

        public static void TogglePlayerMenu()
        {
            if (_instance._openPlayerMenuWindow)
            {
                UiWindowManager.CloseWindow(_instance._openPlayerMenuWindow);
            }
            else
            {
                _instance._openPlayerMenuWindow = UiWindowManager.OpenWindow(_instance._playerMenuWindow);
            }
        }

        public static void ShowDialogue(string[] dialogue, GameObject owner)
        {
            var dialogueWindow = UiWindowManager.OpenWindow(_instance._dialogueWindow);
            dialogueWindow.Setup(dialogue, owner);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
            gameObject.Subscribe<UpdateClientStateMessage>(UpdateClientState);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            switch (msg.State)
            {
                case WorldState.Inactive:
                case WorldState.Loading:
                    foreach (var window in _activeWorldWindows)
                    {
                        UiWindowManager.CloseWindow(window);
                    }
                    break;
                case WorldState.Active:
                    foreach (var window in _activeWorldWindows)
                    {
                        UiWindowManager.OpenWindow(window);
                    }
                    break;
            }
        }

        private void UpdateClientState(UpdateClientStateMessage msg)
        {
            switch (msg.State)
            {
                case WorldClientState.Connecting:
                    foreach (var window in _disconnectedWorldWindows)
                    {
                        UiWindowManager.CloseWindow(window);
                    }
                    break;
                case WorldClientState.Disconnected:
                    foreach (var window in _disconnectedWorldWindows)
                    {
                        UiWindowManager.OpenWindow(window);
                    }
                    break;
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}