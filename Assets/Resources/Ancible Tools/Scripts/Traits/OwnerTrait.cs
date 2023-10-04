using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Owner Trait", menuName = "Ancible Tools/Traits/Ownership/Owner")]
    public class OwnerTrait : Trait
    {
        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryOwnerMessage>(QueryOwner, _instanceId);
        }

        private void QueryOwner(QueryOwnerMessage msg)
        {
            msg.DoAfter.Invoke(_controller.transform.parent.gameObject);
        }
    }
}