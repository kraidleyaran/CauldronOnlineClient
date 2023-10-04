using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Hitbox Trait", menuName = "Ancible Tools/Traits/Network/Network Hitbox")]
    public class NetworkHitboxTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox;
        [SerializeField] private CollisionLayer _collisionLayer;

        private NetworkHitboxController[] _hitboxes = new NetworkHitboxController[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void ApplyHitbox(GameObject target, Trait[] traits)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            foreach (var trait in traits)
            {
                addTraitToUnitMsg.Trait = trait;
                _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, target);
            }

            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupHitboxesMessage>(SetupHitboxes, _instanceId);
        }

        private void SetupHitboxes(SetupHitboxesMessage msg)
        {
            var hitboxes = new List<NetworkHitboxController>();
            foreach (var data in msg.Hitboxes)
            {
                var traits = TraitFactory.GetTraitsByName(data.ApplyOnClient);
                var hitboxController = Instantiate(FactoryController.NETWORK_HITBOX, _controller.transform.parent);

                hitboxController.Setup(_hitbox, _collisionLayer, obj => {ApplyHitbox(obj, traits);});
                hitboxController.SetSize(data.Size.ToVector());
                hitboxController.SetOffset(data.Offset);

                hitboxes.Add(hitboxController);
            }

            _hitboxes = hitboxes.ToArray();
        }

        public override void Destroy()
        {
            foreach (var hitbox in _hitboxes)
            {
                hitbox.Destroy();
                Destroy(hitbox.gameObject);
            }
            base.Destroy();
        }
    }
}