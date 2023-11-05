using Assets.Resources.Ancible_Tools.Scripts.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiAnimationController : MonoBehaviour
    {
        [SerializeField] private Sprite[] _sprites = new Sprite[0];
        [SerializeField] private Sprite _pauseFrame = null;
        [SerializeField] private Image _animationImage;
        [SerializeField] private int _framesBetweenSprites = 1;

        public Material Material => _animationImage.material;

        private Sequence _animationSequence = null;
        private int _currentIndex = 0;
        private bool _playing = false;

        public void WakeUp()
        {
            _animationImage.sprite = _sprites[_currentIndex];
            _animationImage.material = Instantiate(_animationImage.material);
            _playing = true;
            StartAnimationTimer();
        }

        public void TogglePause()
        {
            if (_playing)
            {
                if (_animationSequence != null)
                {
                    if (_animationSequence.IsActive())
                    {
                        _animationSequence.Kill();
                    }

                    _animationSequence = null;
                }

                _currentIndex = 0;
                _animationImage.sprite = _pauseFrame ? _pauseFrame : _sprites[0];
            }
            else if (_animationSequence == null)
            {
                _animationImage.sprite = _sprites[0];
                _currentIndex = 0;
                StartAnimationTimer();
            }

            _playing = !_playing;
        }

        private void StartAnimationTimer()
        {
            _animationSequence = DOTween.Sequence().AppendInterval(_framesBetweenSprites * TickController.TickRate).OnComplete(TimerFinished);
        }

        private void TimerFinished()
        {
            _animationSequence = null;
            _currentIndex++;
            if (_currentIndex >= _sprites.Length)
            {
                _currentIndex = 0;
            }
            _animationImage.sprite = _sprites[_currentIndex];
            StartAnimationTimer();
        }

        void OnDestroy()
        {
            if (_animationSequence != null)
            {
                if (_animationSequence.IsActive())
                {
                    _animationSequence.Kill();
                }

                _animationSequence = null;
            }

            Destroy(_animationImage.material);
        }

    }
}