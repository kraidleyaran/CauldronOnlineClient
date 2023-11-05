using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Math;
using UnityEngine;
using static UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Timers
{
    public class UiTimerManager : UiWindowBase
    {
        private static UiTimerManager _instance = null;

        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private UiTimerController _timerTemplate;

        private static List<UiTimerController> _timers = new List<UiTimerController>();

        void Awake()
        {
            if (_instance)
            {
                UiWindowManager.CloseWindow(this);
                return;
            }

            _instance = this;
        }

        public static void RegisterTimer(int totalTicks, int totalLoops, GameObject owner)
        {
            var timer = Instantiate(_instance._timerTemplate, _instance._rectTransform);
            timer.Setup(totalTicks, totalLoops, owner, owner.transform.position.ToVector2());
            _timers.Add(timer);
        }

        public static void RegisterTimer(int totalTicks, int totalLoops, Vector2 pos)
        {
            var timer = Instantiate(_instance._timerTemplate, _instance._rectTransform);
            timer.Setup(totalTicks, totalLoops, null, pos);
            _timers.Add(timer);
        }

        public static void TimerFinished(UiTimerController timer)
        {
            if (_timers.Contains(timer))
            {
                _timers.Remove(timer);
                Destroy(timer.gameObject);
            }
        }
        
    }
}