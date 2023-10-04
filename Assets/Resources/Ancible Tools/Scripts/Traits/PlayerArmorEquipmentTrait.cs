using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Armor Equipment Trait", menuName = "Ancible Tools/Traits/Player/Player Armor Equipment")]
    public class PlayerArmorEquipmentTrait : Trait
    {
        private Dictionary<ArmorSlot, EquippedArmorItemInstance> _equipped = new Dictionary<ArmorSlot, EquippedArmorItemInstance>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<EquipArmorItemMessage>(EquipArmorItem, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnequipArmorItemFromSlotMessage>(UnequipArmorItemFromSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryArmorEquipmentMessage>(QueryArmorEquipment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetArmorEquipmentMessage>(SetArmorEquipment, _instanceId);
        }

        private void EquipArmorItem(EquipArmorItemMessage msg)
        {
            if (_equipped.TryGetValue(msg.Item.Slot, out var equipped))
            {
                var item = equipped.Item;
                equipped.Unequip(_controller.transform.parent.gameObject);

                var addItemMsg = MessageFactory.GenerateAddItemMsg();
                addItemMsg.Item = item;
                addItemMsg.Stack = 1;
                _controller.gameObject.SendMessageTo(addItemMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(addItemMsg);

                _equipped[msg.Item.Slot] = new EquippedArmorItemInstance(msg.Item, _controller.transform.parent.gameObject);
            }
            else
            {
                _equipped.Add(msg.Item.Slot, new EquippedArmorItemInstance(msg.Item, _controller.transform.parent.gameObject));
            }
        }

        private void UnequipArmorItemFromSlot(UnequipArmorItemFromSlotMessage msg)
        {
            if (_equipped.TryGetValue(msg.Slot, out var equipped))
            {
                var item = equipped.Item;
                equipped.Unequip(_controller.transform.parent.gameObject);
                _equipped.Remove(msg.Slot);

                var addItemMsg = MessageFactory.GenerateAddItemMsg();
                addItemMsg.Item = item;
                addItemMsg.Stack = 1;
                _controller.gameObject.SendMessageTo(addItemMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(addItemMsg);
            }
        }

        private void QueryArmorEquipment(QueryArmorEquipmentMessage msg)
        {
            msg.DoAfter.Invoke(_equipped.Values.ToArray());
        }

        private void SetArmorEquipment(SetArmorEquipmentMessage msg)
        {
            foreach (var item in _equipped)
            {
                if (item.Value != null)
                {
                    item.Value.Unequip(_controller.transform.parent.gameObject);
                }
            }
            _equipped.Clear();
            foreach (var equipData in msg.Equipped)
            {
                var item = ItemFactory.GetItemByName(equipData);
                if (item && item is ArmorItem armor)
                {
                    if (_equipped.ContainsKey(armor.Slot))
                    {
                        _equipped[armor.Slot].Unequip(_controller.transform.parent.gameObject);
                    }

                    _equipped[armor.Slot] = new EquippedArmorItemInstance(armor, _controller.transform.parent.gameObject);
                }
            }
        }
    }
}