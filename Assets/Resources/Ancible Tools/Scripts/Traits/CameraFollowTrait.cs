using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Camera Follow Trait", menuName = "Ancible Tools/Traits/Camera Follow")]
    public class CameraFollowTrait : Trait
    {   
        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdatePositionMessage>(UpdatePosition, _instanceId);
        }

        private void UpdatePosition(UpdatePositionMessage msg)
        {
            CameraController.SetPosition(msg.Position);
            
        }
    }
}