using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Trigger Hitbox Trait", menuName = "Ancible Tools/Traits/Interactable/Network Trigger Hitbox")]
    public class NetworkTriggerHitboxTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox;

        private NetworkHitboxController _hitboxController = null;

        private string[] _triggerEvents = new string[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = Instantiate(FactoryController.NETWORK_HITBOX, _controller.transform.parent);
            _hitboxController.Setup(_hitbox, CollisionLayerFactory.Interaction, Apply);
            SubscribeToMessages();
        }

        private void Apply(GameObject obj)
        {
            if (obj == ObjectManager.Player)
            {
                ClientController.SendToServer(new ClientTriggerEventMessage{TriggerEvents = _triggerEvents});
            }
            
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupNetworkTriggerHitboxMessage>(SetupNetworkTriggerHitbox, _instanceId);
        }

        private void SetupNetworkTriggerHitbox(SetupNetworkTriggerHitboxMessage msg)
        {
            _hitboxController.SetSize(msg.Hitbox.Size.ToVector());
            _triggerEvents = msg.TriggerEvents;
        }
    }
}