using System.Net;
using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiLoginWindow : UiWindowBase
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private InputField _characterNameInput;
        [SerializeField] private InputField _ipAddressInput;
        [SerializeField] private InputField _portInput;

        void Awake()
        {
            _characterNameInput.text = DataController.CurrentCharacter != null ? DataController.CurrentCharacter.Name : string.Empty;
            _ipAddressInput.SetTextWithoutNotify(ClientController.Settings.IpAddres);
            _portInput.SetTextWithoutNotify($"{ClientController.Settings.Port}");
        }

        public void Connect()
        {
            var playerName = _characterNameInput.text.RemoveTags();
            if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(_ipAddressInput.text) && IPAddress.TryParse(_ipAddressInput.text, out var ipAddress) && !string.IsNullOrEmpty(_portInput.text) && int.TryParse(_portInput.text, out var port))
            {
                DataController.CurrentCharacter.Name = playerName;
                ClientController.SetConnctionSettings(_ipAddressInput.text, port);
                ClientController.Connect();
                UiWindowManager.CloseWindow(this);
            }
        }
    }
}