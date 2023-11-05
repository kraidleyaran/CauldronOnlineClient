using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.SystemEvents
{
    public class UiSystemEventTextController : MonoBehaviour
    {
        public RectTransform RectTransform;
        [SerializeField] private Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private int _activeTime = 1800;
        [SerializeField] private int _fadeTime = 30;

        private Sequence _activeTimer = null;
        private Tween _fadeTween = null;

        public void Setup(SystemEvent systemEvent)
        {
            switch (systemEvent.Type)
            {
                default:
                    _text.text = $"{systemEvent.Message}";
                    break;
            }

            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _text.GetHeightOfText(_text.text));
            Show();
        }

        public void Show()
        {
            _canvasGroup.alpha = 1f;
            _activeTimer = DOTween.Sequence().AppendInterval(_activeTime * TickController.TickRate).OnComplete(ActiveTimerFinished);
        }

        private void ActiveTimerFinished()
        {
            _activeTimer = null;
            _fadeTween = _canvasGroup.DOFade(0f, _fadeTime * TickController.TickRate).SetEase(Ease.Linear).OnComplete(FadeCompleted);
        }

        private void FadeCompleted()
        {
            _fadeTween = null;
        }

        void OnDestroy()
        {
            if (_activeTimer != null)
            {
                if (_activeTimer.IsActive())
                {
                    _activeTimer.Kill();
                }

                _activeTimer = null;
            }

            if (_fadeTween != null)
            {
                if (_fadeTween.IsActive())
                {
                    _fadeTween.Kill();
                }

                _fadeTween = null;
            }
        }
    }
}