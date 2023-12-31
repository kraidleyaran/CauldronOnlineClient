﻿using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Combat;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Damage Trait", menuName = "Ancible Tools/Traits/Combat/Damage/Damage")]
    public class DamageTrait : Trait
    {
        public override bool Instant => true;
        public override bool ApplyOnClient => false;

        [SerializeField] private int _amount = 1;
        [SerializeField] private DamageType _type = DamageType.Physical;
        [SerializeField] private BonusTag[] _tags = new BonusTag[0];
        [SerializeField] private Trait[] _applyToOwnerOnDamageDone = new Trait[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            GameObject owner = null;

            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = (damageOrigin) => owner = damageOrigin;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, controller.Sender);
            MessageFactory.CacheMessage(queryOwnerMsg);

            if (owner)
            {
                var id = owner == ObjectManager.Player ? ObjectManager.PlayerObjectId : ObjectManager.GetId(owner);
                var targetId = _controller.transform.parent.gameObject == ObjectManager.Player ? ObjectManager.PlayerObjectId : ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(id) && (id == ObjectManager.PlayerObjectId || targetId == ObjectManager.PlayerObjectId))
                {
                    var amount = _amount;
                    var bonusAmount = 0;
                    var queryBonusDamageMsg = MessageFactory.GenerateQueryDamageBonusMsg();
                    queryBonusDamageMsg.Amount = amount;
                    queryBonusDamageMsg.DoAfter = bonus => bonusAmount += bonus;
                    queryBonusDamageMsg.Tags = _tags;
                    queryBonusDamageMsg.Type = _type;
                    _controller.gameObject.SendMessageTo(queryBonusDamageMsg,owner);
                    MessageFactory.CacheMessage(queryBonusDamageMsg);

                    amount += bonusAmount;
                    var damageDone = false;
                    var takeDamageMsg = MessageFactory.GenerateTakeDamageMsg();
                    takeDamageMsg.Amount = amount;
                    takeDamageMsg.Type = _type;
                    takeDamageMsg.OwnerId = id;
                    takeDamageMsg.OnDamageDone = () => damageDone = true;
                    takeDamageMsg.Tags = _tags;
                    _controller.gameObject.SendMessageTo(takeDamageMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(takeDamageMsg);
                    if (damageDone && _applyToOwnerOnDamageDone.Length > 0)
                    {
                        OnDamageDone(owner);
                    }
                }


            }
        }

        public override string GetDescription()
        {
            if (_tags.Length <= 0)
            {
                return $"{_amount} {_type} Damage";
            }
            else
            {
                return $"{_amount} {_type} Damage ({_tags.ToSingleLine()})";
            }
        }

        private void OnDamageDone(GameObject owner)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            foreach (var trait in _applyToOwnerOnDamageDone)
            {
                addTraitToUnitMsg.Trait = trait;
                _controller.gameObject.SendMessageTo(addTraitToUnitMsg, owner);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

    }
}