using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Loadout
{
    public class UiLoadoutController : MonoBehaviour
    {
        [SerializeField] private UiLoadoutSlotController[] _controllers;

        void Awake()
        {
            RefreshLoadout();
            SubscribeToMessages();
        }

        private void RefreshLoadout()
        {
            var queryLoadoutMsg = MessageFactory.GenerateQueryLoadoutMsg();
            queryLoadoutMsg.DoAfter = UpdateLoadout;
            gameObject.SendMessageTo(queryLoadoutMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryLoadoutMsg);

        }

        private void UpdateLoadout(LoadoutSlot[] slots)
        {
            for (var i = 0; i < slots.Length && i < _controllers.Length; i++)
            {
                var slot = slots[i];
                _controllers[i].Setup(slot, i);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerLoadoutUpdatedMessage>(PlayerLoadoutUpdated);
            gameObject.Subscribe<PlayerInventoryUpdatedMessage>(PlayerInventoryUpdated);
        }

        private void PlayerLoadoutUpdated(PlayerLoadoutUpdatedMessage msg)
        {
            RefreshLoadout();
        }

        private void PlayerInventoryUpdated(PlayerInventoryUpdatedMessage msg)
        {
            foreach (var slot in _controllers)
            {
                slot.RefreshUses();
            }
        }
        
    }
}