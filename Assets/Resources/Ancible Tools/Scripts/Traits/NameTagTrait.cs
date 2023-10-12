using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui.NameTags;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Name Tag Trait", menuName = "Ancible Tools/Traits/General/Name Tag")]
    public class NameTagTrait : Trait
    {
        private string _name = string.Empty;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetNameTagMessage>(SetNameTag, _instanceId);
        }

        private void SetNameTag(SetNameTagMessage msg)
        {
            _name = msg.Name;
            UiNameTagManager.GenerateNameTag(_name, _controller.transform.parent.gameObject);
        }

        public override void Destroy()
        {
            UiNameTagManager.RemoveNameTag(_controller.transform.parent.gameObject);
            base.Destroy();
        }
    }
}