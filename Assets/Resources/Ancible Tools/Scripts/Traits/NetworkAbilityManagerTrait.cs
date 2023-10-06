using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Ability Manager Trait", menuName = "Ancible Tools/Traits/Network/Network Ability Manager")]
    public class NetworkAbilityManagerTrait : Trait
    {
        private WorldAbilityController _abilityController = null;

        private bool _applyTraits = false;

        private UnitState _unitState = UnitState.Active;

        private Vector2Int _faceDirection = Vector2Int.down;
        private WorldVector2Int _worldPosition = WorldVector2Int.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UseAbilityMessage>(UseAbility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateFacingDirectionMessage>(UpdateFacingDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetApplyAbilityMessage>(SetApplyAbility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWorldPositionMessage>(UpdateWorldPosition, _instanceId);
        }

        private void AbilityFinished()
        {
            if (_abilityController)
            {
                _abilityController.Destroy();
                ObjectManager.DestroyNetworkObject(_abilityController.gameObject);
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

        private void UseAbility(UseAbilityMessage msg)
        {
            if (_unitState != UnitState.Dead && _unitState != UnitState.Disabled)
            {
                if (_abilityController)
                {
                    _abilityController.Destroy();
                    ObjectManager.DestroyNetworkObject(_abilityController.gameObject);
                }
                
                var ability = msg.Ability;
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
                _abilityController.Setup(ability, _controller.transform.parent.gameObject, _faceDirection, AbilityFinished, _applyTraits, msg.Ids);
                ObjectManager.RegisterObject(_abilityController.gameObject);
            }
        }

        private void UpdateFacingDirection(UpdateFacingDirectionMessage msg)
        {
            _faceDirection = msg.Direction;
        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState == UnitState.Dead && _abilityController)
            {
                _abilityController.Destroy();
                ObjectManager.DestroyNetworkObject(_abilityController.gameObject);
                _abilityController = null;
            }
        }

        private void SetApplyAbility(SetApplyAbilityMessage msg)
        {
            _applyTraits = msg.ApplyAbility;
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            _worldPosition = msg.Position;
            if (_abilityController)
            {
                _abilityController.SetPosition(_worldPosition.ToWorldVector());
            }
        }

        public override void Destroy()
        {
            if (_abilityController)
            {
                _abilityController.Destroy();
                ObjectManager.DestroyNetworkObject(_abilityController.gameObject);
                _abilityController = null;
            }
            base.Destroy();
        }
    }
}