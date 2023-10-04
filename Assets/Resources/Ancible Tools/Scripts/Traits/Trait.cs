using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Trait", menuName = "Ancible Tools/Traits/General/Trait", order = 0)]
    public class Trait : ScriptableObject
    {
        public int MaxStack = 1;
        public virtual bool Instant => false;
        public virtual bool AddOnCache => false;
        public virtual bool ApplyOnClient => false;
        public virtual bool RequireId => false;

        protected internal TraitController _controller;
        protected internal string _instanceId;
        protected internal string _worldId = string.Empty;

        public virtual void SetupController(TraitController controller)
        {
            _controller = controller;
            _instanceId = _controller.gameObject.GetInstanceID().ToString();
        }

        public void SetWorldId(string worldId)
        {
            _worldId = worldId;
        }

        public virtual void Clear()
        {
            _worldId = string.Empty;
            _controller = null;
            _instanceId = string.Empty;
        }

        public virtual void Destroy()
        {
            if (!Instant)
            {
                _controller.gameObject.UnsubscribeFromAllMessages();
                _controller.transform.parent.gameObject.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }

        }

        public virtual void ResetTrait()
        {

        }

        public virtual string GetDescription()
        {
            return string.Empty;
        }

        public virtual BonusTag[] GetBonusTags()
        {
            return new BonusTag[0];
        }
    }
}