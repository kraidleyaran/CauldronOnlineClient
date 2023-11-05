using System;
using System.Linq;
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
            if (!string.IsNullOrEmpty(_ipAddressInput.text) && !string.IsNullOrEmpty(_portInput.text) && int.TryParse(_portInput.text, out var port))
            {
                var tryConnect = true;
                var ipAddressSaveValue = string.Empty;
                if (IPAddress.TryParse(_ipAddressInput.text, out var ipAddress))
                {
                    ipAddressSaveValue = ipAddress.MapToIPv4().ToString();
                }
                else
                {
                    try
                    {
                        var host = Dns.GetHostEntry(_ipAddressInput.text);
                        ipAddressSaveValue = _ipAddressInput.text;
                        ipAddress = host.AddressList[0].MapToIPv4();
                    }
                    catch (Exception ex)
                    {
                        tryConnect = false;
                        Debug.LogWarning($"Exception while resolving dns - {ex}");
                        UiServerStatusWindow.SetStatusText($"Unable to resolve {_ipAddressInput.text}", true);
                    }
                }

                if (tryConnect)
                {
                    ClientController.SetConnctionSettings(ipAddressSaveValue, port);
                    ClientController.Connect();
                    UiWindowManager.CloseWindow(this);
                }

            }
        }
    }
}