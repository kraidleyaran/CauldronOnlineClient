using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using DG.Tweening;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    public class WorldAbilityController : MonoBehaviour
    {
        public int DownOffset = 8;
        public Vector2Int ObjectOffset = new Vector2Int(0,8);

        private SpriteController _weaponSpriteController = null;

        private GameObject _owner = null;
        private GameObject _attack = null;
        private WorldAbility _ability = null;

        private Sequence _abilitySequence = null;
        private int _currentStep = 0;

        private Action _doAfter = null;
        private bool _applyTraits = false;
        private List<string> _ids = new List<string>();
        private Vector2Int _direction = Vector2Int.zero;

        public void Setup(WorldAbility ability, GameObject owner, Vector2Int direction, Action doAfter, bool applyTraits = false, string[] ids = null)
        {
            _ability = ability;
            _owner = owner;
            _doAfter = doAfter;
            _applyTraits = applyTraits;
            if (ids != null)
            {
                _ids = ids.ToList();
            }

            _attack = AbilityFactory.Ability.GenerateUnitWithParent(transform).gameObject;

            if (_ability.AttackSetup.SwingAnimation)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                addTraitToUnitMsg.Trait = _ability.AttackSetup.SwingAnimation;
                gameObject.SendMessageTo(addTraitToUnitMsg, _attack);
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
            

            var setOwnerMsg = MessageFactory.GenerateSetOwnerMsg();
            setOwnerMsg.Owner = _owner;
            gameObject.SendMessageTo(setOwnerMsg, _attack);
            MessageFactory.CacheMessage(setOwnerMsg);

            var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
            setDirectionMsg.Direction = direction;
            gameObject.SendMessageTo(setDirectionMsg, _attack);
            MessageFactory.CacheMessage(setDirectionMsg);

            var rotation = direction.ToVector2(false).ToZRotation();
            if (_ability.WeaponSprite)
            {
                _weaponSpriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, transform);
                _weaponSpriteController.SetSprite(_ability.WeaponSprite.Sprite);
                _weaponSpriteController.SetScale(_ability.WeaponSprite.Scaling);
            }

            if (direction.x < 0)
            {
                var scale = transform.localScale.ToVector2().ToVector2Int(true);
                scale.x *= -1;
                transform.SetLocalScaling(scale);
            }
            else if (direction.x == 0)
            {
                transform.SetLocalRotation(rotation);
            }

            UpdateStep();

        }

        public void SetPosition(Vector2 pos)
        {
            var position = pos + _direction.ToVector2(false) * (_ability.Offset.ToVector2(true) + FactoryController.ABILITY_CONTROLLER.ObjectOffset.ToVector2(true)) ;
            transform.SetTransformPosition(position);
        }

        private void AdvanceStep()
        {
            _abilitySequence = null;
            if (_currentStep < _ability.AbilitySteps.Length - 1)
            {
                UpdateStep();
            }
            else
            {
                
                _doAfter?.Invoke();
            }
        }

        private void UpdateStep()
        {
            var abilityStep = _ability.AbilitySteps[_currentStep];

            var updateAbilityStateMsg =  MessageFactory.GenerateUpdateAbilityStateMsg();
            updateAbilityStateMsg.State = abilityStep.State;
            gameObject.SendMessageTo(updateAbilityStateMsg, _owner);
            gameObject.SendMessageTo(updateAbilityStateMsg, _attack);
            MessageFactory.CacheMessage(updateAbilityStateMsg);

            var attackStep = _ability.AttackSetup.GetAttackStepByAbilityState(abilityStep.State);
            _weaponSpriteController.transform.SetLocalPosition(attackStep.WeaponSpritePosition.ToVector2(true) + _ability.WeaponSprite.Offset);
            _weaponSpriteController.transform.SetLocalRotation(attackStep.WeaponSpriteRotation);

            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            if (_applyTraits)
            {
                if (abilityStep.ApplyToOwner.Length > 0)
                {
                    foreach (var trait in abilityStep.ApplyToOwner)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        _owner.SendMessageTo(addTraitToUnitMsg, _owner);
                    }
                }

                if (abilityStep.ApplyToAttack.Length > 0)
                {
                    foreach (var trait in abilityStep.ApplyToAttack)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        if (trait.RequireId && _ids.Count > 0)
                        {
                            var id = _ids[0];
                            _ids.RemoveAt(0);
                            addTraitToUnitMsg.WorldId = id;
                        }
                        else
                        {
                            addTraitToUnitMsg.WorldId = string.Empty;
                        }
                        _owner.SendMessageTo(addTraitToUnitMsg, _attack);
                    }
                }

                
            }
            else
            {
                var applyToAttack = abilityStep.ApplyToAttack.Where(a => a.ApplyOnClient).ToArray();
                if (applyToAttack.Length > 0)
                {
                    foreach (var trait in applyToAttack)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        if (trait.RequireId && _ids.Count > 0)
                        {
                            var id = _ids[0];
                            _ids.RemoveAt(0);
                            addTraitToUnitMsg.WorldId = id;
                        }
                        else
                        {
                            addTraitToUnitMsg.WorldId = string.Empty;
                        }
                        gameObject.SendMessageTo(addTraitToUnitMsg, _attack);
                    }
                }
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);

            if (_currentStep < _ability.AbilitySteps.Length - 1)
            {
                _currentStep++;
                _abilitySequence = DOTween.Sequence().AppendInterval(TickController.TickRate * _ability.AbilitySteps[_currentStep].Frames).OnComplete(AdvanceStep);
            }
        }

        public void Destroy()
        {
            if (_abilitySequence != null)
            {
                if (_abilitySequence.IsActive())
                {
                    _abilitySequence.Kill();
                }

                _ability = null;
            }

            _ability = null;
            _owner = null;
            if (_attack)
            {
                Destroy(_attack);
                _attack = null;
            }

            if (_weaponSpriteController)
            {
                Destroy(_weaponSpriteController.gameObject);
                _weaponSpriteController = null;
            }
        }
    }
}