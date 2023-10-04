using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiOverlayWindow : UiWindowBase
    {
        public static UiOverlayWindow _instance = null;

        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private Text _healthText;
        [SerializeField] private UiFillBarController _healthFillbar;
        [SerializeField] private Text _manaText;
        [SerializeField] private UiFillBarController _manaFillbar;
        [SerializeField] private Text _actionText;
        [SerializeField] private Image _actionImage;
        [SerializeField] private Sprite _hasActionSprite;
        [SerializeField] private Sprite _noActionSprite;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            ClearActionText();
            RefreshCombatVitals();
            SubscribeToMessages();
        }

        public static void SetActionText(string text)
        {
            if (_instance)
            {
                _instance._actionText.text = text;
                _instance._actionImage.sprite = _instance._hasActionSprite;
            }
        }

        public static void ClearActionText()
        {
            if (_instance)
            {
                _instance._actionText.text = string.Empty;
                _instance._actionImage.sprite = _instance._noActionSprite;
            }

        }

        private void RefreshCombatVitals()
        {
            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = UpdateCombatVitals;
            gameObject.SendMessageTo(queryCombatStatsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryCombatStatsMsg);
        }

        private void UpdateCombatVitals(CombatStats baseStats, CombatStats bonusStats, CombatVitals vitals, SecondaryStats secondaryStats)
        {
            var combined = baseStats + bonusStats;
            _healthText.text = $"{vitals.Health}/{combined.Health}";
            _healthFillbar.Setup((float)vitals.Health / combined.Health);
            _manaText.text = $"{vitals.Mana}/{combined.Mana}";
            _manaFillbar.Setup((float)vitals.Mana / combined.Mana);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerCombatStatsUpdatedMessage>(PlayerCombatStatsUpdated);
        }

        private void PlayerCombatStatsUpdated(PlayerCombatStatsUpdatedMessage msg)
        {
            RefreshCombatVitals();
        }

        public override void Destroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
            base.Destroy();
        }
    }
}