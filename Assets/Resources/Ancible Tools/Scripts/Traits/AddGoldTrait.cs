using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Add Gold Trait", menuName = "Ancible Tools/Traits/General/Add Gold")]
    public class AddGoldTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _amount = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            var addGoldMsg = MessageFactory.GenerateAddGoldMsg();
            addGoldMsg.Amount = _amount;
            _controller.gameObject.SendMessageTo(addGoldMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(addGoldMsg);
        }


    }
}