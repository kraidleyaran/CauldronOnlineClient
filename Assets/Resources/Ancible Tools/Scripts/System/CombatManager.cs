using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatSettings Settings { get; private set; }

        private static CombatManager _instance = null;

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

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClientWorldSettingsResultMessage>(ClientWorldSettingsResult);
        }

        private void ClientWorldSettingsResult(ClientWorldSettingsResultMessage msg)
        {
            Settings = msg.CombatSettings;
        }
    }
}