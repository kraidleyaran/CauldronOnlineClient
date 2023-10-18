using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hitbox Trait", menuName = "Ancible Tools/Traits/General/Hitbox Trait")]
    public class HitboxTrait : Trait
    {
        public override bool ApplyOnClient => false;

        [SerializeField] private Hitbox.Hitbox _hitbox;
        [SerializeField] private CollisionLayer _collisionLayer;

        [Header("On Enter Events")]
        [SerializeField] private Trait[] _applyToOwnerOnEnter;
        [SerializeField] private Trait[] _applyToTargetOnEnter;
        [SerializeField] private Trait[] _applyToSelfOnEnter;

        [Header("On Exit Events")]
        [SerializeField] private Trait[] _applyToOwnerOnExit;
        [SerializeField] private Trait[] _applyToTargetOnExit;

        private HitboxController _hitboxController = null;

        private GameObject _owner = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, _collisionLayer);

            if (_applyToOwnerOnEnter.Length > 0 || _applyToTargetOnEnter.Length > 0 || _applyToOwnerOnExit.Length > 0 || _applyToTargetOnExit.Length > 0 || _applyToSelfOnEnter.Length > 0)
            {
                var registerCollisionMsg = MessageFactory.GenerateRegisterCollisionMsg();
                registerCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(registerCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(registerCollisionMsg);
            }

            SubscribeToMessages();
        }

        public override string GetDescription()
        {
            var descriptions = _applyToTargetOnEnter.GetTraitDescriptions().ToList();
            descriptions.AddRange(_applyToTargetOnExit.GetTraitDescriptions());
            var description = string.Empty;
            foreach (var trait in descriptions)
            {
                description = string.IsNullOrEmpty(description) ? trait : $"{description}{Environment.NewLine}{trait}";
            }

            return description;
        }

        private void SubscribeToMessages()
        {
            if (_applyToOwnerOnEnter.Length > 0 || _applyToTargetOnEnter.Length > 0)
            {
                _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _instanceId);
            }

            if (_applyToOwnerOnExit.Length > 0 || _applyToTargetOnExit.Length > 0)
            {
                _controller.gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObject, _instanceId);
            }
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateOwnerMessage>(UpdateOwner, _instanceId);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

            var owner = _owner ? _owner : _controller.transform.parent.gameObject;
            if (_applyToOwnerOnEnter.Length > 0)
            {
                foreach (var trait in _applyToOwnerOnEnter)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, owner);
                }
            }

            if (_applyToTargetOnEnter.Length > 0)
            {
                foreach (var trait in _applyToTargetOnEnter)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, msg.Object);
                }
            }

            if (_applyToSelfOnEnter.Length > 0)
            {
                foreach (var trait in _applyToSelfOnEnter)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
            }


            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

            var owner = _owner ? _owner : _controller.transform.parent.gameObject;
            if (_applyToOwnerOnExit.Length > 0)
            {
                foreach (var trait in _applyToOwnerOnExit)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, owner);
                }
            }

            if (_applyToTargetOnExit.Length > 0)
            {
                foreach (var trait in _applyToTargetOnExit)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, msg.Object);
                }
            }


            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void UpdateOwner(UpdateOwnerMessage msg)
        {
            _owner = msg.Owner;
        }
    }
}