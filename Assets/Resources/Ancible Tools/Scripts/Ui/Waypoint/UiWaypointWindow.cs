using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiWaypointWindow : UiWindowBase
    {
        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private UiMenuItemController _recallMenuItem;
        [SerializeField] private UiMenuItemController[] _menuItems = new UiMenuItemController[0];
        [SerializeField] private GameObject _cursor = null;
        [SerializeField] private UiTeleportToPlayerController _teleportToPlayerTemplate;

        private GameObject _owner = null;
        private WorldVector2Int _position = WorldVector2Int.Zero;

        private WorldZone _recallZone = null;
        private WorldVector2Int _recallPosition = WorldVector2Int.Zero;
        private UiTeleportToPlayerController _teleportController = null;

        private int _menuIndex = 0;

        private bool _travelled = false;

        public void Setup(GameObject owner, WorldVector2Int position)
        {
            _owner = owner;
            _position = position;
            _recallMenuItem.SetActive(false);

            var queryRecallMsg = MessageFactory.GenerateQueryRecallMsg();
            queryRecallMsg.DoAfter = UpdateRecall;
            gameObject.SendMessageTo(queryRecallMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryRecallMsg);
            var menuItem = _menuItems[_menuIndex];
            menuItem.SetCursor(_cursor);

            SubscribeToMessages();
        }

        public void BindToCrystal()
        {
            DataController.CurrentCharacter.Zone = WorldZoneManager.CurrentZone.name;
            DataController.CurrentCharacter.Position = _position;
            DataController.SavePlayerData();
        }

        public void Recall()
        {
            _travelled = true;
            gameObject.SendMessageTo(RecallMessage.INSTANCE, ObjectManager.Player);
            UiWindowManager.CloseWindow(this);
        }

        public void Teleport()
        {
            if (!_teleportController)
            {
                _teleportController = Instantiate(_teleportToPlayerTemplate, transform);
                StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                {
                    _teleportController.WakeUp();
                }));
            }
        }

        private void UpdateRecall(WorldZone zone, WorldVector2Int position)
        {
            _recallZone = zone;
            _recallPosition = position;
            _recallMenuItem.SetActive(true);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<TeleportToPlayerClosedMessage>(TeleportToPlayerClosed);
            gameObject.Subscribe<CloseWaypointWindowMessage>(CloseWaypointWindow);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!_teleportController)
            {
                if (!msg.Previous.Green && msg.Current.Green)
                {
                    var menuItem = _menuItems[_menuIndex];
                    if (menuItem.Active)
                    {
                        menuItem.SelectItem();
                    }
                }
                else if (!msg.Previous.Red && msg.Current.Red || !msg.Previous.PlayerMenu && msg.Current.PlayerMenu)
                {
                    UiWindowManager.CloseWindow(this);
                }
                else
                {
                    if (!msg.Previous.Up && msg.Current.Up)
                    {
                        _menuIndex = _menuIndex > 0 ? _menuIndex - 1 : _menuItems.Length - 1;
                        _menuItems[_menuIndex].SetCursor(_cursor);
                    }
                    else if (!msg.Previous.Down && msg.Current.Down)
                    {
                        _menuIndex = _menuIndex < _menuItems.Length - 1 ? _menuIndex + 1 : 0;
                        _menuItems[_menuIndex].SetCursor(_cursor);
                    }

                }
            }
        }

        private void TeleportToPlayerClosed(TeleportToPlayerClosedMessage msg)
        {
            if (_teleportController)
            {
                StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                {
                    Destroy(_teleportController.gameObject);
                    _teleportController = null;
                }));
            }
        }

        private void CloseWaypointWindow(CloseWaypointWindowMessage msg)
        {
            _travelled = msg.Travelling;
            UiWindowManager.CloseWindow(this);
        }

        public override void Close()
        {
            _recallZone = null;
            if (_owner)
            {
                var waypointWindowClosedMsg = MessageFactory.GenerateWaypointWindowClosedMsg();
                waypointWindowClosedMsg.Travelling = _travelled;
                gameObject.SendMessageTo(waypointWindowClosedMsg, _owner);
                MessageFactory.CacheMessage(waypointWindowClosedMsg);
                _owner = null;
            }
            base.Close();
        }
    }
}