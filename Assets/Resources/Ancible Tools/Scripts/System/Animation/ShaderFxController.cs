using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class ShaderFxController : MonoBehaviour
    {
        [SerializeField] protected internal Material _material = null;

        protected internal Action _onFinish = null;
        protected internal SpriteRenderer _renderer = null;
        protected internal Material _baseMaterial = null;

        protected internal void Setup(SpriteRenderer spriteRenderer, Material baseMaterial, Action onFinish)
        {
            _renderer = spriteRenderer;
            _baseMaterial = baseMaterial;
            _renderer.material = _material;
            _onFinish = onFinish;
        }

        public virtual void Destroy()
        {
            _renderer = null;
            _onFinish = null;
        }

        void OnDestroy()
        {
            Destroy();
        }
    }
}