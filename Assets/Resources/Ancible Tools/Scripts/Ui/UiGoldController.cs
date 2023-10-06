using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiGoldController : MonoBehaviour
    {
        [SerializeField] private Text _goldText;

        void Awake()
        {
            RefreshGold();
            SubscribeToMessages();
        }

        private void RefreshGold()
        {
            var queryGoldMsg = MessageFactory.GenerateQueryGoldMsg();
            queryGoldMsg.DoAfter = UpdateGold;
            gameObject.SendMessageTo(queryGoldMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryGoldMsg);
        }

        private void UpdateGold(int amount)
        {
            _goldText.text = $"{amount:n0}";
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerGoldUpdatedMessage>(PlayerGoldUpdated);
        }

        private void PlayerGoldUpdated(PlayerGoldUpdatedMessage msg)
        {
            RefreshGold();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}