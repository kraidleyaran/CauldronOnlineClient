﻿using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Sprite Trait", menuName = "Ancible Tools/Traits/Animation/Sprite")]
    public class SpriteTrait : Trait
    {
        public const string UNIT_ANIMATION_STATE = "Unit Animation State";
        public const string X = "X";
        public const string Y = "Y";
        public const string ABILITY_STATE = "Ability State";

        public Sprite Sprite => _sprite;
        public RuntimeAnimatorController RuntimeController => _runtimeAnimator;
        public Vector2 Scaling => _scaling;
        public SpriteLayer SpriteLayer => _spriteLayer;
        public int SortingOrder => _sortingOrder;
        public bool IgnoreWorldPosition = false;
        public Color ColorMask => _colorMask;
        public float Rotation => _rotation;
        public Vector2 Offset => _offset;

        [SerializeField] private Sprite _sprite;
        [SerializeField] private RuntimeAnimatorController _runtimeAnimator;
        [SerializeField] private Vector2 _scaling = new Vector2(31.25f, 31.25f);
        [SerializeField] private SpriteLayer _spriteLayer;
        [SerializeField] private int _sortingOrder = 0;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] private Color _colorMask = Color.white;
        [SerializeField] private float _rotation = 0f;
        [SerializeField] private FlipSprite _flipX = FlipSprite.None;
        [SerializeField] private Material _material = null;

        private SpriteController _spriteController = null;

        private UnitAnimationState _animationState = UnitAnimationState.Idle;

        private Vector2Int _direction = Vector2Int.down;
        private Vector2Int _faceDirection = Vector2Int.zero;

        private ShaderFxController _currentShader = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
            if (_material)
            {
                _spriteController.SpriteRenderer.material = Instantiate(_material);
            }
            if (_runtimeAnimator)
            {
                _spriteController.SetRuntimeController(_runtimeAnimator);
            }
            else
            {
                _spriteController.SetSprite(_sprite);
            }
            _spriteController.SetScale(_scaling);
            if (IgnoreWorldPosition)
            {
                _spriteController.SetSortingOrder(_sortingOrder);
            }
            else
            {
                var sortingOrder = _controller.transform.position.ToVector2().ToWorldPosition().Y * -1;
                _spriteController.SetSortingOrder(sortingOrder + _sortingOrder);
            }
            if (_spriteLayer)
            {
                _spriteController.SetSortingLayerFromSpriteLayer(_spriteLayer);
            }
            _spriteController.SetOffset(_offset);
            _spriteController.SetColorMask(_colorMask);
            _spriteController.SetRotation(_rotation);
            _spriteController.SetDirection(Vector2Int.down);
            SubscribeToMessages();
        }

        private void FlashColorFinished()
        {
            _currentShader.Destroy();
            Destroy(_currentShader.gameObject);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitAnimationStateMessage>(SetUnitAnimationState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyFlashColorFxMessage>(ApplyFlashColorFx, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAbilityStateMessage>(UpdateAbilityState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetFacingDirectionMessage>(SetFacingDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryFacingDirectionMessage>(QueryFacingDirection, _instanceId);
            if (!IgnoreWorldPosition)
            {
                _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWorldPositionMessage>(UpdateWorldPosition, _instanceId);
            }
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDefaulMaterialsMessage>(SetDefaultMaterials, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSpriteColorDataMessage>(SetSpriteColorData, _instanceId);
        }


        private void SetUnitAnimationState(SetUnitAnimationStateMessage msg)
        {
            if (_animationState != msg.State)
            {
                _animationState = msg.State;
                if (_animationState == UnitAnimationState.Attack)
                {
                    _spriteController.SetAnimatorState(ABILITY_STATE, (float)AbilityState.Backswing);
                }
                _spriteController.SetAnimatorState(UNIT_ANIMATION_STATE, (float)msg.State);
            }
        }

        private void UpdateDirection(UpdateDirectionMessage msg)
        {
            if (msg.Direction != Vector2Int.zero && _direction != msg.Direction)
            {
                _direction = msg.Direction;
                var faceDirection = _direction;
                if (_direction.x > 0 || _direction.x < 0)
                {
                    faceDirection = _direction.x > 0 ? Vector2Int.right : Vector2Int.left;
                }
                else
                {
                    faceDirection = _direction.y > 0 ? Vector2Int.up : Vector2Int.down;
                }

                if (_faceDirection != faceDirection)
                {
                    _faceDirection = faceDirection;
                    var updateFaceDirectionMsg = MessageFactory.GenerateUpdateFacingDirectionMsg();
                    updateFaceDirectionMsg.Direction = _faceDirection;
                    _controller.gameObject.SendMessageTo(updateFaceDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(updateFaceDirectionMsg);
                }

                _spriteController.SetAnimatorState(X, _faceDirection.x);
                _spriteController.SetAnimatorState(Y, _faceDirection.y);

                if (_faceDirection.x != 0 && _flipX != FlipSprite.None )
                {
                    switch (_flipX)
                    {
                        case FlipSprite.Negative:
                            _spriteController.FlipX(_faceDirection.x < 0);
                            break;
                        case FlipSprite.Positive:
                            _spriteController.FlipX(_faceDirection.x > 0);
                            break;
                    }
                }
            }
        }

        private void ApplyFlashColorFx(ApplyFlashColorFxMessage msg)
        {
            if (_currentShader)
            {
                _currentShader.Destroy();
                Destroy(_currentShader.gameObject);
            }

            var controller = Instantiate(ShaderFxFactory.FlashColor, _controller.transform);
            _currentShader = controller;

            controller.Setup(_spriteController.SpriteRenderer, _spriteController.SpriteRenderer.material, FlashColorFinished, msg.Color, msg.FramesBetweenFlashes, msg.Loops);
        }

        private void UpdateAbilityState(UpdateAbilityStateMessage msg)
        {
            if (_runtimeAnimator)
            {
                _spriteController.SetAnimatorState(ABILITY_STATE, (float)msg.State);
            }
        }

        private void SetFacingDirection(SetFacingDirectionMessage msg)
        {
            if (_faceDirection != msg.Direction)
            {
                _faceDirection = msg.Direction;

                var updateFacingDirectionMsg = MessageFactory.GenerateUpdateFacingDirectionMsg();
                updateFacingDirectionMsg.Direction = _faceDirection;
                _controller.gameObject.SendMessageTo(updateFacingDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateFacingDirectionMsg);

                if (_runtimeAnimator)
                {
                    _spriteController.SetAnimatorState(X, _faceDirection.x);
                    _spriteController.SetAnimatorState(Y, _faceDirection.y);
                }

                if (_faceDirection.x != 0 && _flipX != FlipSprite.None)
                {
                    switch (_flipX)
                    {
                        case FlipSprite.Negative:
                            _spriteController.FlipX(_faceDirection.x < 0);
                            break;
                        case FlipSprite.Positive:
                            _spriteController.FlipX(_faceDirection.x > 0);
                            break;
                    }
                }
            }
        }

        private void QueryFacingDirection(QueryFacingDirectionMessage msg)
        {
            msg.DoAfter.Invoke(_direction);
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            _spriteController.SetSortingOrder((msg.Position.Y * -1) + _sortingOrder);
        }

        private void SetDefaultMaterials(SetDefaulMaterialsMessage msg)
        {
            _spriteController.SpriteRenderer.materials = msg.Default;
        }

        private void SetSpriteColorData(SetSpriteColorDataMessage msg)
        {
            var hair = ColorOptionFactory.GetOptionByName(msg.Data.Hair);
            if (hair)
            {
                hair.Apply(_spriteController.SpriteRenderer.material, false);
            }

            var eyes = ColorOptionFactory.GetOptionByName(msg.Data.Eyes);
            if (eyes)
            {
                eyes.Apply(_spriteController.SpriteRenderer.material, false);
            }

            var primaryShirt = ColorOptionFactory.GetOptionByName(msg.Data.PrimaryShirt);
            if (primaryShirt)
            {
                primaryShirt.Apply(_spriteController.SpriteRenderer.material, false);
            }

            var secondaryShirt = ColorOptionFactory.GetOptionByName(msg.Data.SecondaryShirt);
            if (secondaryShirt)
            {
                secondaryShirt.Apply(_spriteController.SpriteRenderer.material, true);
            }
        }

        public override void Destroy()
        {
            if (_currentShader)
            {
                _currentShader.Destroy();
                _currentShader = null;
            }

            if (_material)
            {
                Destroy(_spriteController.SpriteRenderer.material);
            }
            base.Destroy();
        }
    }
}