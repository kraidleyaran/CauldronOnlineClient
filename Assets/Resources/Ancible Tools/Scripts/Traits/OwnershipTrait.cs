using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
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
        }

        private void QueryOwner(QueryOwnerMessage msg)
        {
            msg.DoAfter.Invoke(_owner);
        }

        
    }
}