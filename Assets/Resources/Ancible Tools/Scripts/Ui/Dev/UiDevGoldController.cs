using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Dev
{
    public class UiDevGoldController : MonoBehaviour
    {
        [SerializeField] private InputField _goldInput;

        public void GiveGold()
        {
            if (int.TryParse(_goldInput.text, out var gold))
            {
                var addGoldMsg = MessageFactory.GenerateAddGoldMsg();
                addGoldMsg.Amount = gold;
                gameObject.SendMessageTo(addGoldMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(addGoldMsg);
            }
        }
    }
}