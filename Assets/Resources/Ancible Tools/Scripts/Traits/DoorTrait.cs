using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Door Trait", menuName = "Ancible Tools/Traits/Interactable/Door")]
    public class DoorTrait : InteractableTrait
    {
        protected internal override string _actionText => "Open";
        private bool _open = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        protected internal override void Interact()
        {
            if (!_open)
            {
                var targetId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(targetId))
                {
                    ClientController.SendToServer(new ClientDoorCheckMessage{TargetId = targetId, Tick = TickController.ServerTick});
                }
            }
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupDoorMessage>(SetupDoor, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDoorStateMessage>(SetDoorState, _instanceId);
        }

        private void SetupDoor(SetupDoorMessage msg)
        {
            _open = msg.Open;
            _controller.transform.parent.SetLocalRotation(msg.Rotation);
            _controller.transform.parent.gameObject.SetActive(!_open);
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }

        private void SetDoorState(SetDoorStateMessage msg)
        {
            _open = msg.Open;
            _controller.transform.parent.gameObject.SetActive(!_open);
        }
    }
}