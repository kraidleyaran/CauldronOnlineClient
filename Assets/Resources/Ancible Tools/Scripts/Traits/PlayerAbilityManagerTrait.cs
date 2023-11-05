using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using DG.Tweening;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Player Ability Manager Trait", menuName = "Ancible Tools/Traits/Player/Player Ability Manager")]
    public class PlayerAbilityManagerTrait : Trait
    {
        [SerializeField] private WorldAbility[] _startingAbilities = new WorldAbility[0];

        private WorldAbilityController _abilityController = null;

        private UnitState _unitState = UnitState.Active;
        private Vector2Int _faceDirection = Vector2Int.down;
        private WorldVector2Int _worldPosition = WorldVector2Int.Zero;

        private List<WorldAbility> _abilities = new List<WorldAbility>();
        private Dictionary<WorldAbility, Sequence> _cooldowns = new Dictionary<WorldAbility, Sequence>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            foreach (var ability in _startingAbilities)
            {
                if (!_abilities.Contains(ability))
                {
                    _abilities.Add(ability);
                }
            }
            SubscribeToMessages();
        }

        private void AbilityFinished()
        {
            if (_abilityController)
            {
                _abilityController.Destroy();
                Destroy(_abilityController.gameObject);
                _abilityController = null;
            }
            if (_unitState == UnitState.Attack)
            {
                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Active;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }

        }

        private void CooldownFinished(WorldAbility ability)
        {
            if (_cooldowns.ContainsKey(ability))
            {
                _cooldowns.Remove(ability);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UseAbilityMessage>(UseAbility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFacingDirectionMessage>(UpdateFacingDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWorldPositionMessage>(UpdateWorldPosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAbilitiesMessage>(QueryAbilities, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAbilityCooldownMessage>(QueryAbilityCooldown, _instanceId);
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState == UnitState.Dead && _abilityController)
            {
                _abilityController.Destroy();
                Destroy(_abilityController.gameObject);
            }

        }

        private void UpdateFacingDirection(UpdateFacingDirectionMessage msg)
        {
            _faceDirection = msg.Direction;
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            _worldPosition = msg.Position;
            if (_abilityController)
            {
                _abilityController.SetPosition(_worldPosition.ToWorldVector());
            }
        }

        private void UseAbility(UseAbilityMessage msg)
        {
            if (_unitState != UnitState.Attack && _unitState != UnitState.Dead && _unitState != UnitState.Interaction)
            {
                var ability = msg.Ability;
                if (ability.RequiredResources.Length > 0)
                {
                    var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                    removeItemMsg.Update = false;
                    foreach (var item in ability.RequiredResources)
                    {
                        removeItemMsg.Item = item.Item;
                        removeItemMsg.Stack = item.Stack;
                        _controller.gameObject.SendMessageTo(removeItemMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(removeItemMsg);
                    _controller.gameObject.SendMessage(PlayerInventoryUpdatedMessage.INSTANCE);
                }

                if (ability.ManaCost > 0)
                {
                    var manaReduction = 0;
                    var totalStats = new CombatStats();
                    var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
                    queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, vitals, secondaryBonus) =>
                    {
                        totalStats = baseStats + bonusStats;
                        manaReduction = secondaryBonus.ManaReduction;
                    };
                    _controller.gameObject.SendMessageTo(queryCombatStatsMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(queryCombatStatsMsg);

                    manaReduction += CombatManager.Settings.CalculateManaReduction(totalStats);
                    var manaCost = Mathf.Max(1, ability.ManaCost - manaReduction);

                    var removeManaMsg = MessageFactory.GenerateRemoveManaMsg();
                    removeManaMsg.Amount = manaCost;
                    _controller.gameObject.SendMessageTo(removeManaMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(removeManaMsg);
                }

                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Attack;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                setUnitAnimationStateMsg.State = UnitAnimationState.Attack;
                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                var abilityOffset = ability.Offset.ToVector2(true);
                var objOffset = FactoryController.ABILITY_CONTROLLER.ObjectOffset.ToVector2(true);
                var pos = _worldPosition.ToWorldVector() + new Vector2(abilityOffset.x * _faceDirection.x, abilityOffset.y * _faceDirection.y) + objOffset;
                if (_faceDirection.y < 0)
                {
                    pos.y += FactoryController.ABILITY_CONTROLLER.DownOffset * DataController.Interpolation;
                }

                _abilityController = Instantiate(FactoryController.ABILITY_CONTROLLER, pos, Quaternion.identity);
                var ids = new string[ability.GetRequiredIdCount()];
                for (var i = 0; i < ids.Length; i++)
                {
                    ids[i] = $"{ObjectManager.PlayerObjectId}-{Guid.NewGuid().ToString()}";
                }
                _abilityController.Setup(ability, _controller.transform.parent.gameObject, _faceDirection, AbilityFinished, true, ids);

                if (ability.Cooldown > 0)
                {
                    var cooldownTime = ability.Cooldown * TickController.WorldTick;
                    _cooldowns.Add(ability, DOTween.Sequence().AppendInterval(cooldownTime).OnComplete(() => { CooldownFinished(ability);}));
                }

                ClientController.SendToServer(new ClientUseAbilityMessage { Ability = ability.name, Direction = _faceDirection.ToWorldVector(), Position = _worldPosition, Tick = TickController.ServerTick, Ids = ids });
            }

        }

        private void QueryAbilities(QueryAbilitiesMessage msg)
        {
            msg.DoAfter?.Invoke(_abilities.ToArray());
        }

        private void QueryAbilityCooldown(QueryAbilityCooldownMessage msg)
        {
            msg.DoAfter.Invoke(_cooldowns.TryGetValue(msg.Ability, out var cooldown) ? cooldown : null);
        }

        public override void Destroy()
        {
            if (_abilityController)
            {
                _abilityController.Destroy();
                Destroy(_abilityController.gameObject);
                
            }
            _abilities.Clear();
            if (_cooldowns.Count > 0)
            {
                var cooldowns = _cooldowns.Values.ToArray();
                foreach (var cooldown in cooldowns)
                {
                    if (cooldown != null)
                    {
                        if (cooldown.IsActive())
                        {
                            cooldown.Kill();
                        }
                    }
                }
                _cooldowns.Clear();
            }
            base.Destroy();
        }
    }
}