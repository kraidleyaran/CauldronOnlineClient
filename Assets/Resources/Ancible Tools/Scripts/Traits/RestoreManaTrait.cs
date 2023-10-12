using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Restore Mana Trait", menuName = "Ancible Tools/Traits/Combat/Restore Mana")]
    public class RestoreManaTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private IntNumberRange _amount;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var restoreManaMsg = MessageFactory.GenereateRestoreManaMsg();
            restoreManaMsg.Amount = _amount.Roll();
            _controller.gameObject.SendMessageTo(restoreManaMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(restoreManaMsg);
        }

        public override string GetDescription()
        {
            var amountString = _amount.Minimum >= _amount.Maximum ? $"{_amount.Minimum}" : $"{_amount}";
            return $"+{amountString} Mana";
        }
    }
}