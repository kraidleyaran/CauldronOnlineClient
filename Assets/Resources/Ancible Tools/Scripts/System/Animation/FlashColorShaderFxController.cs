using System;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class FlashColorShaderFxController : ShaderFxController
    {
        private Sequence _flashSequence = null;
        private bool _colorActive = false;
        private int _framesBetweenFlashes = 0;
        private int _loops = 0;
        private int _loopCount = 0;

        public void Setup(SpriteRenderer spriteRenderer, Action onFinish, Color color, int framesBetween, int loops)
        {
            Setup(spriteRenderer, onFinish);
            _framesBetweenFlashes = framesBetween;
            _loops = loops;
            _renderer.material.SetColor("_ChangeColor", color);
            FlashColor();
        }

        private void FlashColor()
        {
            _colorActive = !_colorActive;
            if (_loops >= 0 && !_colorActive)
            {
                _loopCount++;
            }
            if (_loops < 0 || _loopCount <= _loops)
            {
                if (_renderer.material.shader == _material.shader)
                {
                    _renderer.material.SetInt("_Active", _colorActive ? 1 : 0);
                }
                _flashSequence = DOTween.Sequence().AppendInterval(_framesBetweenFlashes * TickController.TickRate).OnComplete(
                    () =>
                    {
                        _flashSequence = null;
                        FlashColor();
                    });
            }
            else
            {
                _onFinish?.Invoke();
            }

        }

        public override void Destroy()
        {
            if (_flashSequence != null)
            {
                if (_flashSequence.IsActive())
                {
                    _flashSequence.Kill();
                }

                _flashSequence = null;
            }
            _renderer?.material.SetInt("_Active", 0);
            _renderer = null;
            base.Destroy();
        }
    }
}