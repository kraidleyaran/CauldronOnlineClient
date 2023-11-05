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

        private bool _active = true;

        public void Setup(SpriteRenderer spriteRenderer, Material baseMaterial, Action onFinish, Color color, int framesBetween, int loops)
        {
            Setup(spriteRenderer, baseMaterial, onFinish);
            _framesBetweenFlashes = framesBetween;
            _loops = loops;
            _material.SetColor("_ChangeColor", color);
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
                _active = !_active;
                _renderer.material = _active ? _material : _baseMaterial;
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

            if (_renderer)
            {
                _renderer.material = _baseMaterial;
                _renderer = null;
            }
            
            base.Destroy();
        }
    }
}