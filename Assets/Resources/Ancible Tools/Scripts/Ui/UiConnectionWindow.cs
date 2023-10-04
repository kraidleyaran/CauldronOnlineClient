using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiConnectionWindow : UiWindowBase
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Text _connectionStateText = null;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Text _pingText;
        [SerializeField] private Text _versionText;

        [Header("State Colors")]
        [SerializeField] private Color _connected = Color.green;
        [SerializeField] private Color _connecting = Color.yellow;
        [SerializeField] private Color _disconnected = Color.red;

        void Awake()
        {
            _disconnectButton.gameObject.SetActive(false);
            _connectionStateText.text = StaticMethods.ApplyColorToText($"{WorldClientState.Disconnected}", _disconnected);
            _versionText.text = $"v{Application.version}";
            SubscribeToMessages();
        }

        void Update()
        {
            var ping = ClientController.GetAveragePing();
            _pingText.text = $"{ping}ms";
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateClientStateMessage>(UpdateClientState);
        }

        private void UpdateClientState(UpdateClientStateMessage msg)
        {
            var text = $"{msg.State}";
            var color = _disconnected;
            switch (msg.State)
            {
                case WorldClientState.Disconnected:
                    _disconnectButton.gameObject.SetActive(false);
                    break;
                case WorldClientState.Connecting:
                    color = _connecting;
                    _disconnectButton.gameObject.SetActive(true);
                    break;
                case WorldClientState.Connected:
                    color = _connected;
                    break;
            }

            text = $"{StaticMethods.ApplyColorToText(text, color)}";
            _connectionStateText.text = text;
        }

        public void Connect()
        {
            ClientController.Connect();
        }

        public void Disconnect()
        {
            ClientController.Disconnect();
        }

    }
}