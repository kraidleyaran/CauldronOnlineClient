using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    public class InteractableTrait : Trait
    {
        public const string INTERACT = "Interact";

        [SerializeField] protected internal Hitbox.Hitbox _hitbox;


        protected internal HitboxController _hitboxController;
        protected internal virtual string _actionText => INTERACT;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.Interaction);
            _hitboxController.AddSubscriber(_controller.gameObject);
            SubscribeToMessages();
        }

        protected internal virtual void Interact()
        {

        }

        protected internal virtual void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _instanceId);
            _controller.gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObject, _instanceId);
            _controller.gameObject.SubscribeWithFilter<InteractMessage>(Interact, _instanceId);
        }

        private void Interact(InteractMessage msg)
        {
            Interact();
        }

        protected internal virtual void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (msg.Object == ObjectManager.Player)
            {
                var setInteractionMsg = MessageFactory.GenerateSetInteractionMsg();
                setInteractionMsg.Action = _actionText;
                setInteractionMsg.Interaction = _controller.gameObject;
                _controller.gameObject.SendMessageTo(setInteractionMsg, msg.Object);
                MessageFactory.CacheMessage(setInteractionMsg);
            }
        }

        protected internal virtual void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            if (msg.Object == ObjectManager.Player)
            {
                var removeInteractionMsg = MessageFactory.GenerateRemoveInteractionMsg();
                removeInteractionMsg.Interaction = _controller.gameObject;
                _controller.gameObject.SendMessageTo(removeInteractionMsg, msg.Object);
                MessageFactory.CacheMessage(removeInteractionMsg);
            }
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
            }

            _hitboxController = null;
            base.Destroy();
        }
    }
}