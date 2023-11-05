using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Timers
{
    public class UiTimerController : MonoBehaviour
    {
        [SerializeField] private UiFillBarController _fillBar;
        [SerializeField] private Vector2Int _offset = new Vector2Int(0,8);

        private GameObject _owner = null;
        private Vector2 _position = Vector2.zero;

        private int _totalTicks = 0;
        private int _totalLoops = 0;
        private int _currentLoops = 0;

        private Sequence _timerSequence = null;

        public void Setup(int ticks, int loops, GameObject obj, Vector2 pos)
        {
            _owner = obj;
            UpdatePosition();
            _totalTicks = ticks;
            _totalLoops = loops;
            StartTimer();
            SubscribeToMessages();
        }

        private void UpdatePosition()
        {
            if (_owner)
            {
                var position = CameraController.Camera.WorldToScreenPoint(_owner.transform.position.ToVector2()).ToVector2();
                transform.SetTransformPosition(position + _offset);
            }
            else
            {
                var position = CameraController.Camera.WorldToScreenPoint(_position).ToVector2();
                transform.SetLocalPosition(position + _offset);
            }

        }

        private void StartTimer()
        {
            _timerSequence = DOTween.Sequence().AppendInterval(_totalTicks * TickController.WorldTick).OnUpdate(UpdateTimer).OnComplete(TimerFinished);
        }

        private void UpdateTimer()
        {
            var totalTicks = _totalTicks * (_totalLoops + 1);
            var currentTicks = _currentLoops * _totalTicks;
            currentTicks += Mathf.RoundToInt(_timerSequence.position / TickController.WorldTick);
            var percentComplete = (float) currentTicks / totalTicks;
            _fillBar.Setup(1f - percentComplete);
        }

        private void TimerFinished()
        {
            _timerSequence = null;
            _currentLoops++;
            if (_currentLoops < _totalLoops)
            {
                StartTimer();
            }
            else
            {
                UiTimerManager.TimerFinished(this);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            UpdatePosition();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}