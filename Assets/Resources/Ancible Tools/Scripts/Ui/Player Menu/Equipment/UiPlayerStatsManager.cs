using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Combat;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Equipment
{
    public class UiPlayerStatsManager : MonoBehaviour
    {
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _manaText;
        [SerializeField] private Text _armorText;
        [SerializeField] private Text _strengthText;
        [SerializeField] private Text _agilityText;
        [SerializeField] private Text _wisdomText;
        [SerializeField] private Text _luckText;

        [Header("Secondary Stats")]
        [SerializeField] private Text _physicalDamageText;
        [SerializeField] private Text _magicalDamageText;
        [SerializeField] private Text _physicalDefenseText;
        [SerializeField] private Text _magicalDefenseText;
        [SerializeField] private Text _physicalCritText;
        [SerializeField] private Text _magicalCritText;
        [SerializeField] private Text _manaReductionText;


        void Awake()
        {
            RefreshCombatStats();
            SubscribeToMessages();
        }

        private string FormatStatText(int total, int bonus)
        {
            var bonusText = string.Empty;
            if (bonus != 0)
            {
                var color = bonus > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat;
                bonusText = $"({StaticMethods.ApplyColorToText($"{bonus.ToStatString()}", color)})";
                return $"{total} {bonusText}";
            }
            return $"{total}";


        }

        private string FormatStatText(float total, float bonus)
        {
            var bonusText = string.Empty;
            if (bonus > 0f || bonus < 0f)
            {
                var color = bonus > 0f ? ColorFactory.PositiveStat : ColorFactory.NegativeStat;
                bonusText = $"({StaticMethods.ApplyColorToText($"{bonus.ToStatString()}", color)})";
                return $"{total:P} {bonusText}";
            }
            return $"{total:P}";


        }

        private void RefreshCombatStats()
        {
            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = UpdateCombatStats;
            gameObject.SendMessageTo(queryCombatStatsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryCombatStatsMsg);
        }

        private void UpdateCombatStats(CombatStats baseStats, CombatStats bonusStats, CombatVitals vitals, SecondaryStats bonusSecondary)
        {
            var combined = baseStats + bonusStats;
            _healthText.text = FormatStatText(combined.Health, bonusStats.Health);
            _manaText.text = FormatStatText(combined.Mana, bonusStats.Mana);
            _armorText.text = FormatStatText(combined.Armor, bonusStats.Armor);
            _strengthText.text = FormatStatText(combined.Strength, bonusStats.Strength);
            _agilityText.text = FormatStatText(combined.Agility, bonusStats.Agility);
            _wisdomText.text = FormatStatText(combined.Wisdom, bonusStats.Wisdom);
            _luckText.text = FormatStatText(combined.Luck, bonusStats.Luck);

            _physicalDamageText.text = FormatStatText(CombatManager.Settings.CalculateDamageBonus(DamageType.Physical, combined), bonusSecondary.PhysicalDamage);
            _magicalDamageText.text = FormatStatText(CombatManager.Settings.CalculateDamageBonus(DamageType.Magical, combined), bonusSecondary.MagicalDamage);
            _physicalDefenseText.text = FormatStatText(CombatManager.Settings.CalculateDamageResist(DamageType.Physical, combined), bonusSecondary.PhysicalDefense);
            _magicalDefenseText.text = FormatStatText(CombatManager.Settings.CalculateDamageResist(DamageType.Magical, combined), bonusSecondary.MagicalDefense);
            _physicalCritText.text = FormatStatText(CombatManager.Settings.CalculateCriticalStrike(DamageType.Physical, combined), bonusSecondary.PhysicalCrit);
            _magicalCritText.text = FormatStatText(CombatManager.Settings.CalculateCriticalStrike(DamageType.Magical, combined), bonusSecondary.MagicalCrit);
            _manaReductionText.text = FormatStatText(CombatManager.Settings.CalculateManaReduction(combined), bonusSecondary.ManaReduction);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerCombatStatsUpdatedMessage>(PlayerCombatStatsUpdated);
        }

        private void PlayerCombatStatsUpdated(PlayerCombatStatsUpdatedMessage msg)
        {
            RefreshCombatStats();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}