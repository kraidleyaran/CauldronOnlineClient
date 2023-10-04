using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Swing Animation Trait", menuName = "Ancible Tools/Traits/Animation/Swing Animation")]
    public class SwingAnimationTrait : Trait
    {
        [SerializeField] private Sprite _backSwing;
        [SerializeField] private Sprite _startup;
        [SerializeField] private Sprite _active;
        [SerializeField] private Sprite _recovery;
        [SerializeField] private Vector2 _scale = new Vector2(31.25f, 31.25f);

        private SpriteController _spriteController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
            _spriteController.SetScale(_scale);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAbilityStateMessage>(UpdateAbilityState, _instanceId);
        }

        private void UpdateAbilityState(UpdateAbilityStateMessage msg)
        {
            Sprite sprite = null;
            switch (msg.State)
            {
                case AbilityState.Backswing:
                    sprite = _backSwing;
                    break;
                case AbilityState.Startup:
                    sprite = _startup;
                    break;
                case AbilityState.Active:
                    sprite = _active;
                    break;
                case AbilityState.Recovery:
                    sprite = _recovery;
                    break;
            }

            _spriteController.SetSprite(sprite);
            _spriteController.gameObject.SetActive(sprite);
           
        }

        public override void Destroy()
        {
            if (_spriteController)
            {
                Destroy(_spriteController.gameObject);
                _spriteController = null;
            }
            base.Destroy();
        }
    }
}