using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui.FloatingText;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Combat;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Combat Stats Trait", menuName = "Ancible Tools/Traits/Combat/Combat Stats")]
    public class CombatStatsTrait : Trait
    {
        public const string FULL_HEAL = "Full Heal!";

        [SerializeField] private Trait[] _applyOnDamageTaken = new Trait[0];
        [SerializeField] private bool _reportHeal = false;

        private bool _reportDamage = false;

        private CombatStats _baseStats = new CombatStats();
        private CombatStats _bonusStats = new CombatStats();
        private SecondaryStats _bonusSecondary = new SecondaryStats();

        private CombatVitals _vitals = new CombatVitals();

        private UnitState _unitState = UnitState.Active;

        private bool _isMonster = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCombatStatsMessage>(SetCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCombatStatsMessage>(QueryCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<FullHealMessage>(FullHeal, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyCombatStatsMessage>(ApplyCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<HealMessage>(Heal, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDamageBonusMessage>(QueryDamageBonus, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplySecondaryStatsMessage>(ApplySecondaryStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RemoveManaMessage>(RemoveMana, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<IsMonsterMessage>(IsMonster, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RestoreManaMessage>(RestoreMana, _instanceId);
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            if (_unitState != UnitState.Dead)
            {
                var amount = msg.Amount;
                if (_reportDamage && !msg.Event)
                {
                    var totalStats = _baseStats + _bonusStats;
                    var resisted = CombatManager.Settings.CalculateDamageResist(msg.Type, totalStats);
                    switch (msg.Type)
                    {
                        case DamageType.Physical:
                            resisted += _bonusSecondary.PhysicalDefense;
                            break;
                        case DamageType.Magical:
                            resisted += _bonusSecondary.MagicalDefense;
                            break;
                    }
                    amount -= resisted;
                    if (amount <= 0)
                    {
                        amount = 1;
                    }
                    if (_controller.transform.parent.gameObject == ObjectManager.Player)
                    {
                        UiFloatingTextManager.Register($"{StaticMethods.ApplyColorToText($"-{amount}", ColorFactory.PlayerDamageTaken)}", _controller.transform.parent.gameObject);
                        ClientController.SendToServer(new ClientDamageMessage { OwnerId = msg.OwnerId, TargetId = ObjectManager.PlayerObjectId, Amount = amount, Tick = TickController.ServerTick });
                    }
                    else
                    {
                        var id = ObjectManager.GetId(_controller.transform.parent.gameObject);
                        if (!string.IsNullOrEmpty(id) && ObjectManager.PlayerObjectId == msg.OwnerId)
                        {
                            if (_isMonster)
                            {
                                UiFloatingTextManager.Register($"{StaticMethods.ApplyColorToText($"{amount}", ColorFactory.MonsterDamageTaken)}", _controller.transform.parent.gameObject);
                            }
                            ClientController.SendToServer(new ClientDamageMessage { OwnerId = msg.OwnerId, TargetId = id, Amount = amount, Tick = TickController.ServerTick });
                        }
                    }
                }

                _vitals.Health -= amount;
                msg.OnDamageDone?.Invoke();
                _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
                if (_applyOnDamageTaken.Length > 0)
                {
                    var addtraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in _applyOnDamageTaken)
                    {
                        addtraitToUnitMsg.Trait = trait;
                        _controller.transform.parent.gameObject.SendMessageTo(addtraitToUnitMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(addtraitToUnitMsg);
                }

                if (_vitals.Health <= 0 && _reportDamage)
                {
                    _controller.gameObject.SendMessageTo(UnitDeathMessage.INSTANCE, _controller.transform.parent.gameObject);
                }
            }
        }

        private void SetCombatStats(SetCombatStatsMessage msg)
        {
            _baseStats = msg.Stats;
            _vitals = msg.Vitals;
            _bonusSecondary = msg.BonusSecondary;
            _reportDamage = msg.Report;
            if (_vitals.Health <= 0)
            {
                _controller.gameObject.SendMessageTo(UnitDeathMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void QueryCombatStats(QueryCombatStatsMessage msg)
        {
            msg.DoAfter.Invoke(_baseStats, _bonusStats, _vitals, _bonusSecondary);
        }

        private void FullHeal(FullHealMessage msg)
        {
            var combined = _baseStats + _bonusStats;
            _vitals.Health = combined.Health;
            _vitals.Mana = combined.Mana;
            _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
            UiFloatingTextManager.Register($"{StaticMethods.ApplyColorToText(FULL_HEAL, ColorFactory.Heal)}", _controller.transform.parent.gameObject);
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void ApplyCombatStats(ApplyCombatStatsMessage msg)
        {
            if (msg.Bonus)
            {
                _bonusStats += msg.Stats;
            }
            else
            {
                _baseStats += msg.Stats;
            }

            if (msg.Stats.Health != 0 || msg.Stats.Mana != 0)
            {
                var combined = _baseStats + _bonusStats;
                
                if (msg.Stats.Health != 0)
                {
                    _vitals.Health = Mathf.Max(0, Mathf.Min(_vitals.Health + msg.Stats.Health, combined.Health));
                }

                if (msg.Stats.Mana != 0)
                {
                    _vitals.Mana = Mathf.Max(0, Mathf.Min(_vitals.Mana + msg.Stats.Mana, combined.Mana));
                }
            }

            _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);

        }

        private void Heal(HealMessage msg)
        {
            var combined = _baseStats + _bonusStats;
            _vitals.Health = Mathf.Max(0, Mathf.Min(_vitals.Health + msg.Amount, combined.Health));
            _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
            if (_reportHeal && !msg.IsEvent)
            {
                GameObject owner = null;
                var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
                queryOwnerMsg.DoAfter = healOwner => owner = healOwner;
                _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.Sender);
                MessageFactory.CacheMessage(queryOwnerMsg);

                var ownerId = ObjectManager.PlayerObjectId;
                if (owner && ownerId != ObjectManager.PlayerObjectId)
                {
                    ownerId = ObjectManager.GetId(owner);
                }

                var targetId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (string.IsNullOrEmpty(targetId) && _controller.transform.parent.gameObject == ObjectManager.Player)
                {
                    targetId = ObjectManager.PlayerObjectId;
                }

                if (!string.IsNullOrEmpty(ownerId) && !string.IsNullOrEmpty(targetId))
                {
                    ClientController.SendToServer(new ClientHealMessage{Amount = msg.Amount, OwnerId = ownerId, TargetId = targetId, Tick = TickController.ServerTick});
                }
            }

            UiFloatingTextManager.Register($"{StaticMethods.ApplyColorToText($"{msg.Amount}", ColorFactory.Heal)}", _controller.transform.parent.gameObject);
        }

        private void QueryDamageBonus(QueryDamageBonusMessage msg)
        {
            var combined = _baseStats + _bonusStats;
            var bonus = CombatManager.Settings.CalculateDamageBonus(msg.Type, combined);
            var critChance = CombatManager.Settings.CalculateCriticalStrike(msg.Type, combined);
            switch (msg.Type)
            {
                case DamageType.Physical:
                    bonus += _bonusSecondary.PhysicalDamage;
                    critChance += _bonusSecondary.PhysicalCrit;
                    break;
                case DamageType.Magical:
                    bonus += _bonusSecondary.MagicalDamage;
                    critChance += _bonusSecondary.MagicalCrit;
                    break;
            }
            
            var critRoll = Random.Range(0f, 1f);
            if (critChance >= critRoll)
            {
                bonus += CombatManager.Settings.CalculateCriticalStrikeDamage(msg.Amount);
            }
            msg.DoAfter.Invoke(bonus);
        }

        private void ApplySecondaryStats(ApplySecondaryStatsMessage msg)
        {
            _bonusSecondary += msg.Stats;
            _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void RemoveMana(RemoveManaMessage msg)
        {
            var combined = _baseStats + _bonusStats;
            _vitals.Mana = Mathf.Max(0, Mathf.Min(_vitals.Mana - msg.Amount, combined.Mana));
            _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void IsMonster(IsMonsterMessage msg)
        {
            _isMonster = true;
        }

        private void RestoreMana(RestoreManaMessage msg)
        {
            var combined = _baseStats + _bonusStats;
            _vitals.Mana = Mathf.Max(0, Mathf.Min(_vitals.Mana + msg.Amount, combined.Mana));
            _controller.gameObject.SendMessageTo(CombatStatsUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
        }
    }
}