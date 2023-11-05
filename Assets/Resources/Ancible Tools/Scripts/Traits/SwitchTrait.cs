using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
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
        private HitboxController _combatHitboxController = null;

        private SpriteTrait[] _signals = new SpriteTrait[0];
        private int _currentSignal = 0;
        private bool _locked = false;
        private bool _lockOnInteract = false;
        private object _combatReceiver = new object();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
        }

        protected internal override void Interact()
        {
            if (!_locked)
            {
                var signal = _currentSignal + 1;
                if (signal >= _signals.Length)
                {
                    signal = 0;
                }

                if (_lockOnInteract)
                {
                    _locked = true;
                }
                var targetId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                ClientController.SendToServer(new ClientSwitchSignalMessage { Signal = signal, TargetId = targetId, Locked = _locked, Tick = TickController.ServerTick });
            }

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
            _lockOnInteract = msg.LockOnInteract;
            _locked = msg.Locked;
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
            if (msg.CombatInteractable)
            {
                _combatHitboxController = Instantiate(_hitboxController, _controller.transform.parent);
                _combatHitboxController.Setup(_hitbox, CollisionLayerFactory.MonsterHurt);
                _combatHitboxController.AddSubscriber(_spriteController.gameObject);
                _spriteController.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(CombatEnterCollisionWithObject, _instanceId);
            }
        }

        private void SetSignal(SetSignalMessage msg)
        {
            SetSignal(msg.Signal);
            _locked = msg.Locked;
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            var addOrder = _signals[_currentSignal].SortingOrder;
            _spriteController.SetSortingOrder((msg.Position.Y * -1) + addOrder);
        }

        private void CombatEnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            Interact();
        }

        public override void Destroy()
        {
            if (_combatHitboxController)
            {
                Destroy(_combatHitboxController.gameObject);
                _combatHitboxController = null;
            }
            _spriteController.gameObject.UnsubscribeFromAllMessages();
            base.Destroy();
        }
    }
}