using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Ownership Trait", menuName = "Ancible Tools/Traits/Ownership/Ownership")]
    public class OwnershipTrait : Trait
    {
        private GameObject _owner;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetOwnerMessage>(SetOwner, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryOwnerMessage>(QueryOwner, _instanceId);
        }

        private void SetOwner(SetOwnerMessage msg)
        {
            _owner = msg.Owner;
            var updateOwnerMsg = MessageFactory.GenerateUpdateOwnerMsg();
            updateOwnerMsg.Owner = _owner;
            _controller.gameObject.SendMessageTo(updateOwnerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateOwnerMsg);
        }

        private void QueryOwner(QueryOwnerMessage msg)
        {
            msg.DoAfter.Invoke(_owner);
        }

        
    }
}