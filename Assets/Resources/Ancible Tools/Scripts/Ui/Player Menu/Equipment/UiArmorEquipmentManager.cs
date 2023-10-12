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
        [SerializeField] private GridLayoutGroup _grid;

        private Dictionary<ArmorSlot, UiArmorItemController> _controllers = new Dictionary<ArmorSlot, UiArmorItemController>();

        private Vector2Int _cursorPosition = Vector2Int.zero;
        private bool _hover = false;

        void Awake()
        {
            var position = Vector2Int.zero;
            foreach (var controller in _equipmentControllers)
            {
                if (!_controllers.ContainsKey(controller.Slot))
                {
                    _controllers.Add(controller.Slot, controller);

                    controller.Position = position;
                    position.x++;
                    if (position.x >= _grid.constraintCount)
                    {
                        position.x = 0;
                        position.y++;
                    }
                }
            }
            RefreshArmorEquipment();
            SubscribeToMessages();
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

            var cursorController = _controllers.Values.FirstOrDefault(c => c.Position == _cursorPosition);
            if (!cursorController)
            {
                cursorController = _controllers[ArmorSlot.Helm];
            }
            cursorController.SetCursor(_cursor);
            cursorController.SetHover(_hover);

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerEquipmentUpdatedMessage>(PlayerEquipmentUpdated);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var buttonPushed = false;
            var selected = _equipmentControllers.FirstOrDefault(c => c.Position == _cursorPosition);
            if (!msg.Previous.Red && msg.Current.Red)
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
            else if (!msg.Previous.Info && msg.Current.Info)
            {
                _hover = !_hover;
                if (selected)
                {
                    selected.SetHover(_hover);
                }
            }

            if (!buttonPushed)
            {
                var direction = Vector2Int.zero;
                if (!msg.Previous.Up && msg.Current.Up)
                {
                    direction.y = -1;
                }
                else if (!msg.Previous.Down && msg.Current.Down)
                {
                    direction.y = 1;
                }
                else if (!msg.Previous.Left && msg.Current.Left)
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