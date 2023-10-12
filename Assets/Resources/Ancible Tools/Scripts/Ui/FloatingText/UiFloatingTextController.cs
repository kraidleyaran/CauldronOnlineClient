using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.FloatingText
{
    public class UiFloatingTextController : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private int _jumpTime;
        [SerializeField] private int _jumpPower;
        [SerializeField] private int _jumps;
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private Vector2 _worldPositionOffset = Vector2.zero;
        [SerializeField] private Vector2 _localPosition = Vector2.zero;
        [SerializeField] private int _activeFrames = 1;
        
        private Tween _tween = null;

        private GameObject _owner = null;
        private Vector2 _previousOwnerPosition = Vector2.zero;

        public void Setup(string text, GameObject owner)
        {
            _text.text = text;
            _tween = _text.rectTransform.DOLocalJump(_localPosition, _jumpPower * DataController.Interpolation, _jumps, _jumpTime * TickController.TickRate).SetEase(_ease).OnComplete(CompleteJump);
            _owner = owner;
            _previousOwnerPosition = _owner.transform.position.ToVector2();
            var pos = CameraController.Camera.WorldToScreenPoint(_previousOwnerPosition).ToVector2();
            transform.SetTransformPosition(pos + _worldPositionOffset);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_owner)
            {
                _previousOwnerPosition = _owner.transform.position.ToVector2();
            }
            var pos = CameraController.Camera.WorldToScreenPoint(_previousOwnerPosition).ToVector2();
            transform.SetTransformPosition(pos + _worldPositionOffset);
        }

        private void CompleteJump()
        {
            _tween = DOTween.Sequence().AppendInterval(_activeFrames * TickController.TickRate).OnComplete(FinishActivate);
        }

        private void FinishActivate()
        {
            _tween = null;
            UiFloatingTextManager.Unregister(this);
        }

        void OnDestroy()
        {
            _owner = null;
            gameObject.UnsubscribeFromAllMessages();
            if (_tween != null)
            {
                if (_tween.IsActive())
                {
                    _tween.Kill();
                }

                _tween = null;
            }
        }
    }
}