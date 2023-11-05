using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Visual Fx Trait", menuName = "Ancible Tools/Traits/Animation/Fx/Visual Fx")]
    public class VisualFxTrait : Trait
    {
        public override bool Instant => _instantiateAlone;

        [SerializeField] private RuntimeAnimatorController _runtime;
        [SerializeField] private Vector2 _scale = new Vector2(31.25f,31.25f);
        [SerializeField] private Vector2Int _offset = Vector2Int.zero;
        [SerializeField] private bool _instantiateAlone = false;
        [SerializeField] private bool _applySorting = false;
        [SerializeField] private SpriteLayer _spriteLayer;
        [SerializeField] private int _sortOrder = 0;

        private VisualFxController _fxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var offset = _offset.ToVector2(true);
            VisualFxController fxController = null;
            if (_instantiateAlone)
            {
                var pos = _controller.transform.position.ToVector2() + offset;
                fxController = Instantiate(FactoryController.VISUAL_FX, pos, Quaternion.identity);
                fxController.Setup(_runtime, null);
            }
            else
            {
                _fxController = Instantiate(FactoryController.VISUAL_FX, _controller.transform.parent);
                _fxController.transform.SetLocalPosition(offset);
                fxController = _fxController;
                fxController.Setup(_runtime, VisualFxFinished);
            }

            if (_applySorting)
            {
                var sortingOrder = fxController.transform.position.ToVector2().ToWorldPosition().Y * -1;
                fxController.SetSortingLayer(_spriteLayer);
                fxController.SetSortingOrder(sortingOrder + _sortOrder);
            }
            fxController.transform.SetLocalScaling(_scale);
            
        }

        private void VisualFxFinished()
        {
            _fxController.Destroy();
            Destroy(_fxController.gameObject);
            var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitFromUnitByControllerMsg.Controller = _controller;
            _controller.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
        }

        public override void Destroy()
        {
            if (_fxController)
            {
                _fxController.Destroy();
                Destroy(_fxController.gameObject);
                _fxController = null;
            }
            base.Destroy();
        }
    }
}