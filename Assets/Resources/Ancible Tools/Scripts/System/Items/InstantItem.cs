using Assets.Resources.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Instant World Item", menuName = "Ancible Tools/Items/Instant Item")]
    public class InstantItem : WorldItem
    {
        public override ItemType Type => ItemType.Instant;

        [SerializeField] private Trait[] _applyOnPickup;

        public void Apply(GameObject owner)
        {
            if (_applyOnPickup.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyOnPickup)
                {
                    addTraitToUnitMsg.Trait = trait;
                    owner.SendMessageTo(addTraitToUnitMsg, owner);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
        }
    }
}