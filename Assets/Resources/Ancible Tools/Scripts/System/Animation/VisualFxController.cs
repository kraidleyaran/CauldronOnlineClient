using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class VisualFxController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;

        private Action _doAfter = null;

        public void Setup(RuntimeAnimatorController runtime, Action doAfter)
        {
            _animator.runtimeAnimatorController = runtime;
            _doAfter = doAfter;
            _animator.enabled = true;
            _animator.Play(0);
        }

        public void VisualFxFinished()
        {
            if (_doAfter != null)
            {
                _doAfter?.Invoke();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetSortingLayer(SpriteLayer layer)
        {
            _spriteRenderer.sortingLayerID = layer.Id;
        }

        public void SetSortingOrder(int order)
        {
            _spriteRenderer.sortingOrder = order;
        }


        public void Destroy()
        {
            _animator.enabled = false;
            _animator.runtimeAnimatorController = null;
            _doAfter = null;
        }
    }
}