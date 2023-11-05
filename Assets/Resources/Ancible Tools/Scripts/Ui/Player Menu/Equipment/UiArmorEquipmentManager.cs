using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Equipment
{
    public class UiArmorEquipmentManager : MonoBehaviour
    {
        [SerializeField] private UiArmorItemController[] _equipmentControllers = new UiArmorItemController[0];
        [SerializeField] private GameObject _cursor;
        [SerializeField] private HorizontalLayoutGroup _grid;
        [SerializeField] private GameObject _unequip;

        private Dictionary<ArmorSlot, UiArmorItemController> _controllers = new Dictionary<ArmorSlot, UiArmorItemController>();

        private Vector2Int _cursorPosition = Vector2Int.zero;
        private bool _hover = false;
        private bool _active = false;

        public void WakeUp()
        {
            var position = Vector2Int.zero;
            foreach (var controller in _equipmentControllers)
            {
                if (!_controllers.ContainsKey(controller.Slot))
                {
                    _controllers.Add(controller.Slot, controller);

                    controller.Position = position;
                    position.x++;
                    //if (position.x >= _grid.)
                    //{
                    //    position.x = 0;
                    //    position.y++;
                    //}
                }
            }
            _unequip.gameObject.SetActive(false);
            RefreshArmorEquipment();
            SubscribeToMessages();
        }

        public void SetActive(bool active)
        {
            _active = active;
            var selected = _controllers.Values.FirstOrDefault(c => c.Position == _cursorPosition);
            if (_active)
            {
                if (selected)
                {
                    selected.SetCursor(_cursor);
                    selected.SetHover(_hover);
                    _unequip.gameObject.SetActive(selected.Item != null);
                }
            }
            else if (selected)
            {
                selected.SetHover(false);
                _unequip.gameObject.SetActive(false);
            }
        }

        private void RefreshArmorEquipment()
        {
            var queryArmorEquipmentMsg = MessageFactory.GenerateQueryArmorEquipmentMsg();
            queryArmorEquipmentMsg.DoAfter = UpdateArmorEquipment;
            gameObject.SendMessageTo(queryArmorEquipmentMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryArmorEquipmentMsg);
        }

        private void UpdateArmorEquipment(EquippedArmorItemInstance[] items)
        {
            foreach (var controller in _equipmentControllers)
            {
                controller.Setup(null);
                controller.SetHover(false);
            }
            foreach (var item in items)
            {
                if (_controllers.TryGetValue(item.Item.Slot, out var controller))
                {
                    controller.Setup(item);
                }
            }

            if (_active)
            {
                var cursorController = _controllers.Values.FirstOrDefault(c => c.Position == _cursorPosition);
                if (!cursorController)
                {
                    cursorController = _controllers[ArmorSlot.Helm];
                }
                cursorController.SetCursor(_cursor);
                cursorController.SetHover(_hover);
                _unequip.gameObject.SetActive(cursorController.Item != null);
            }


        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerEquipmentUpdatedMessage>(PlayerEquipmentUpdated);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var selected = _equipmentControllers.FirstOrDefault(c => c.Position == _cursorPosition);
            if (!msg.Previous.Info && msg.Current.Info)
            {
                _hover = !_hover;
                if (selected && _active)
                {
                    selected.SetHover(_hover);
                }
            }
            if (_active)
            {
                var buttonPushed = false;
                
                if (!msg.Previous.Green && msg.Current.Green)
                {

                    if (selected && selected.Item != null)
                    {
                        var unequipArmorItemMsg = MessageFactory.GenerateUnequipArmorItemFromSlotMsg();
                        unequipArmorItemMsg.Slot = selected.Item.Item.Slot;
                        gameObject.SendMessageTo(unequipArmorItemMsg, ObjectManager.Player);
                        MessageFactory.CacheMessage(unequipArmorItemMsg);
                        buttonPushed = true;
                    }
                }

                if (!buttonPushed)
                {
                    var direction = Vector2Int.zero;
                    //if (!msg.Previous.Up && msg.Current.Up)
                    //{
                    //    direction.y = -1;
                    //}
                    //else if (!msg.Previous.Down && msg.Current.Down)
                    //{
                    //    direction.y = 1;
                    //}
                    if (!msg.Previous.Left && msg.Current.Left)
                    {
                        direction.x = -1;
                    }
                    else if (!msg.Previous.Right && msg.Current.Right)
                    {
                        direction.x = 1;
                    }

                    if (direction != Vector2Int.zero)
                    {
                        var controller = _equipmentControllers.FirstOrDefault(c => c.Position == _cursorPosition + direction);
                        if (controller)
                        {
                            if (selected)
                            {
                                selected.SetHover(false);
                            }
                            _cursorPosition = controller.Position;
                            controller.SetCursor(_cursor);
                            controller.SetHover(_hover);
                            _unequip.gameObject.SetActive(controller.Item != null);
                        }
                    }
                }
            }
            
            
        }

        private void PlayerEquipmentUpdated(PlayerEquipmentUpdatedMessage msg)
        {
            RefreshArmorEquipment();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}