using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Heal Trait", menuName = "Ancible Tools/Traits/Combat/Heal")]
    public class HealTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private WorldIntRange _amount;
        [SerializeField] private bool _applyMagicBonus;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            GameObject owner = null;
            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = healOwner => owner = healOwner;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.Sender);
            MessageFactory.CacheMessage(queryOwnerMsg);
            var amount = _amount.Roll(true);

            var ownerId = string.Empty;
            if (owner)
            {
                ownerId = ObjectManager.Player == owner ? ObjectManager.PlayerObjectId : ObjectManager.GetId(owner);
                if (_applyMagicBonus)
                {
                    //TODO: Apply magic bonus;
                    //var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
                    //queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, vitals) =>
                    //{
                        
                    //}
                }
            }
            else
            {
                owner = _controller.transform.parent.gameObject;
            }

            var healMsg = MessageFactory.GenerateHealMsg();
            healMsg.OwnerId = ownerId;
            healMsg.IsEvent = false;
            healMsg.Amount = amount;
            owner.SendMessageTo(healMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(healMsg);
        }
    }
}