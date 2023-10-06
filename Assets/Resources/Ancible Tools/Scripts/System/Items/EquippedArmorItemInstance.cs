using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class EquippedArmorItemInstance
    {
        public ArmorItem Item;
        public int CharmSlot = 0;

        private TraitController[] _applied = new TraitController[0];

        public EquippedArmorItemInstance(ArmorItem item, GameObject owner)
        {
            Item = item;
            _applied = item.Equip(owner);
        }

        public void Unequip(GameObject owner)
        {
            if (_applied.Length > 0)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                foreach (var trait in _applied)
                {
                    removeTraitFromUnitByControllerMsg.Controller = trait;
                    owner.SendMessageTo(removeTraitFromUnitByControllerMsg, owner);
                }
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }
        }
    }
}