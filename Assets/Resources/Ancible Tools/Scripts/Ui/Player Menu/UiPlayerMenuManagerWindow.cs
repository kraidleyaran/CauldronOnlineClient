using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu
{
    public class UiPlayerMenuManagerWindow : UiWindowBase
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private int _startingMenuIndex = 0;
        [SerializeField] private UiPlayerMenu[] _playerMenus = new UiPlayerMenu[0];
        [SerializeField] private Text _leftMenuText;
        [SerializeField] private Text _selectedMenuText;
        [SerializeField] private Text _rightMenuText;
        [SerializeField] private RectTransform _baseTransform;

        private UiPlayerMenu _currentMenu = null;

        private int _menuIndex = 0;

        void Awake()
        {
            if (_startingMenuIndex < _playerMenus.Length)
            {
                _menuIndex = _startingMenuIndex;
            }
            RefreshMenus();
            SubscribeToMessages();
        }

        private void RefreshMenus()
        {
            var previousMenuIndex = _menuIndex > 0 ? _menuIndex - 1 : _playerMenus.Length - 1;
            var nextMenuIndex = _menuIndex < _playerMenus.Length - 1 ? _menuIndex + 1 : 0;
            _leftMenuText.text = $"{_playerMenus[previousMenuIndex].DisplayName}";
            _selectedMenuText.text = $"{_playerMenus[_menuIndex].DisplayName}";
            _rightMenuText.text = $"{_playerMenus[nextMenuIndex].DisplayName}";

            if (_currentMenu)
            {
                var newMenu = _playerMenus[_menuIndex];
                if (newMenu.name != _currentMenu.DisplayName)
                {
                    _currentMenu.Destroy();
                    Destroy(_currentMenu.gameObject);
                    _currentMenu = Instantiate(_playerMenus[_menuIndex], _baseTransform);
                    _currentMenu.name = _playerMenus[_menuIndex].name;
                }
            }
            else
            {
                _currentMenu = Instantiate(_playerMenus[_menuIndex], _baseTransform);
                _currentMenu.name = _playerMenus[_menuIndex].name;
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.LeftShoulder && msg.Current.LeftShoulder)
            {
                var menuIndex = _menuIndex > 0 ? _menuIndex - 1 : _playerMenus.Length - 1;
                if (menuIndex != _menuIndex)
                {
                    _menuIndex = menuIndex;
                    RefreshMenus();
                }
                
            }
            else if (!msg.Previous.RightShoulder && msg.Current.RightShoulder)
            {
                var menuIndex = _menuIndex < _playerMenus.Length - 1 ? _menuIndex + 1 : 0;
                if (menuIndex != _menuIndex)
                {
                    _menuIndex = menuIndex;
                    RefreshMenus();
                }
            }
        }

        public override void Destroy()
        {
            if (_currentMenu)
            {
                _currentMenu.Destroy();
            }

            _currentMenu = null;
            base.Destroy();
        }
    }
}