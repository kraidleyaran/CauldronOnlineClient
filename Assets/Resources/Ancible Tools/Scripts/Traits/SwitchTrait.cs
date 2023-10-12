using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Switch Trait", menuName = "Ancible Tools/Traits/Interactable/Switches/Switch")]
    public class SwitchTrait : InteractableTrait
    {
        protected internal override string _actionText => "Interact";

        private SpriteController _spriteController = null;

        private SpriteTrait[] _signals = new SpriteTrait[0];
        private int _currentSignal = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
        }

        protected internal override void Interact()
        {
            var signal = _currentSignal + 1;
            if (signal >= _signals.Length)
            {
                signal = 0;
            }

            var targetId = ObjectManager.GetId(_controller.transform.parent.gameObject);
            ClientController.SendToServer(new ClientSwitchSignalMessage{Signal = signal, TargetId = targetId, Tick = TickController.ServerTick});
        }

        private void SetSignal(int signal)
        {
            _currentSignal = signal;
            if (_currentSignal < _signals.Length)
            {
                var currentSignal = _signals[_currentSignal];
                _spriteController.SetSprite(currentSignal.Sprite);
                _spriteController.SetScale(currentSignal.Scaling);
                _spriteController.transform.SetLocalPosition(currentSignal.Offset);
            }

        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupSwitchMessage>(SetupSwitch, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSignalMessage>(SetSignal, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWorldPositionMessage>(UpdateWorldPosition, _instanceId);
        }

        private void SetupSwitch(SetupSwitchMessage msg)
        {
            var sprites = new List<SpriteTrait>();
            foreach (var signal in msg.Signals)
            {
                var sprite = TraitFactory.GetSprite(signal);
                if (sprite)
                {
                    sprites.Add(sprite);
                }
            }

            _signals = sprites.ToArray();
            SetSignal(msg.CurrentSignal);
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }

        private void SetSignal(SetSignalMessage msg)
        {
            SetSignal(msg.Signal);
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            var addOrder = _signals[_currentSignal].SortingOrder;
            _spriteController.SetSortingOrder((msg.Position.Y * -1) + addOrder);
        }
    }
}