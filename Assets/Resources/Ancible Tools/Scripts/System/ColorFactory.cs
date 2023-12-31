﻿using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ColorFactory : MonoBehaviour
    {
        public static Color PositiveStat => _instance._positiveStat;
        public static Color NegativeStat => _instance._negativeStat;
        public static Color PlayerDamageTaken => _instance._playerDamageTaken;
        public static Color MonsterDamageTaken => _instance._monsterDamageTaken;
        public static Color Heal => _instance._healTaken;
        public static Color ManaRestored => _instance._manaRestored;

        private static ColorFactory _instance = null;

        [SerializeField] private Color _positiveStat = Color.green;
        [SerializeField] private Color _negativeStat = Color.red;

        [Header("Floating Text")]
        [SerializeField] private Color _playerDamageTaken = Color.red;
        [SerializeField] private Color _monsterDamageTaken = Color.white;
        [SerializeField] private Color _manaRestored = Color.blue;
        [SerializeField] private Color _healTaken = Color.green;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }
    }
}