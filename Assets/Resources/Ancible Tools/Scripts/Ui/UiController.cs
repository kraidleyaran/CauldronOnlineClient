using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Characters;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Dev;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.Ui.PlayerRoster;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Shop;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiController : MonoBehaviour
    {
        public static UiShopWindow Shop => _instance._shopWindow;
        public static UiCraftingWindow Crafting => _instance._craftingWindow;
        public static UiWaypointWindow Waypoint => _instance._waypointWindow;
        public static UiCharacterManagerWindow CharacterManager => _instance._characterManagerWindow;
        public static UiCreateCharacterWindow CreateCharacter => _instance._createCharacterWindow;
        public static UiPromptWindow Prompt => _instance._promptWindow;

        private static UiController _instance = null;

        [SerializeField] private UiWindowBase[] _disconnectedWorldWindows = new UiWindowBase[0];
        [SerializeField] private UiWindowBase[] _activeWorldWindows = new UiWindowBase[0];
        [SerializeField] private UiWindowBase[] _alwaysOpenWindows = new UiWindowBase[0];
        [SerializeField] private UiPlayerMenuManagerWindow _playerMenuWindow = null;
        [SerializeField] private UiShopWindow _shopWindow;
        [SerializeField] private UiDialogueWindow _dialogueWindow;
        [SerializeField] private UiCraftingWindow _craftingWindow;
        [SerializeField] private UiWaypointWindow _waypointWindow;
        [SerializeField] private UiCharacterManagerWindow _characterManagerWindow;
        [SerializeField] private UiCreateCharacterWindow _createCharacterWindow;
        [SerializeField] private UiPromptWindow _promptWindow;
        [SerializeField] private UiPlayerRosterWindow _rosterWindow;
        [SerializeField] private UiDevWindow _devWindow;
        
        

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

            foreach (var window in _alwaysOpenWindows)
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

        public static void ToggleRoster()
        {
            if (UiWindowManager.IsWindowOpen(_instance._rosterWindow.name))
            {
                UiWindowManager.CloseWindow(_instance._rosterWindow);
            }
            else
            {
                UiWindowManager.OpenWindow(_instance._rosterWindow);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
            gameObject.Subscribe<UpdateClientStateMessage>(UpdateClientState);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
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

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (Debug.isDebugBuild && !msg.Previous.DevWindow && msg.Current.DevWindow)
            {
                if (UiWindowManager.IsWindowOpen(_devWindow.name))
                {
                    UiWindowManager.CloseWindow(_devWindow);
                }
                else
                {
                    UiWindowManager.OpenWindow(_devWindow);
                }
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}