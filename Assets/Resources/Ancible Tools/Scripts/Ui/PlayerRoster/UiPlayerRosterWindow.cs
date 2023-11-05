using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.PlayerRoster
{
    public class UiPlayerRosterWindow : UiWindowBase
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private UiPlayerRosterItemController _playerTemplate;
        [SerializeField] private VerticalLayoutGroup _grid;
        [SerializeField] private int _maxViewable = 5;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;
        [SerializeField] private GameObject _cursor;

        private RegisteredPlayerData[] _players = new RegisteredPlayerData[0];
        private UiPlayerRosterItemController[] _controllers = new UiPlayerRosterItemController[0];
        private int _dataIndex = 0;
        private int _cursorPosition = 0;

        void Awake()
        {
            UpdatePlayers();
            SubscribeToMessages();
        }

        private void UpdatePlayers()
        {
            var refresh = ClientController.Roster.Length != _players.Length;
            _players = ClientController.Roster.ToArray();
            if (refresh)
            {
                RefreshPlayers();
            }
            else
            {
                foreach (var controller in _controllers)
                {
                    controller.UpdateData();
                }
            }

        }

        private void RefreshPlayers()
        {
            _cursor.transform.SetParent(transform);
            _cursor.SetActive(false);

            foreach (var controller in _controllers)
            {
                Destroy(controller.gameObject);
            }
            _controllers = new UiPlayerRosterItemController[0];
            _dataIndex = Mathf.Max(0,Mathf.Min(_players.Length - _maxViewable, _dataIndex));
            var max = Mathf.Min(_maxViewable, _players.Length);
            var controllers = new List<UiPlayerRosterItemController>();
            for (var i = 0; i < max; i++)
            {
                var index = i + _dataIndex;
                var controller = Instantiate(_playerTemplate, _grid.transform);
                controller.Setup(_players[index]);
                controllers.Add(controller);
            }
            _controllers = controllers.OrderBy(c => c.Player.DisplayName).ToArray();
            if (_controllers.Length > 0)
            {
                if (_cursorPosition > _controllers.Length - 1)
                {
                    _cursorPosition = _controllers.Length - 1;
                }
                _controllers[_cursorPosition].SetCursor(_cursor);
            }
            else
            {
                _cursor.SetActive(false);
            }
            

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<PlayerRosterUpdatedMessage>(PlayerRosterUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Down && msg.Current.Down)
            {
                if (_cursorPosition < _controllers.Length)
                {
                    _cursorPosition++;
                    _controllers[_cursorPosition].SetCursor(_cursor);
                }
                else if (_dataIndex < _players.Length - _maxViewable)
                {
                    _dataIndex++;
                    RefreshPlayers();
                }
            }
            else if (!msg.Previous.Up && msg.Current.Up)
            {
                if (_cursorPosition > 0)
                {
                    _cursorPosition--;
                    _controllers[_cursorPosition].SetCursor(_cursor);
                }
                else if (_dataIndex > 0)
                {
                    _dataIndex--;
                    RefreshPlayers();
                }
            }
            else if (!msg.Previous.PlayerMenu && msg.Current.PlayerMenu || !msg.Previous.Red && msg.Current.Red)
            {
                UiWindowManager.CloseWindow(this);
            }
        }

        private void PlayerRosterUpdated(PlayerRosterUpdatedMessage msg)
        {
            UpdatePlayers();
        }

        public override void Close()
        {
            gameObject.SendMessage(RosterWindowClosedMessage.INSTANCE);
            base.Close();
        }
    }
}