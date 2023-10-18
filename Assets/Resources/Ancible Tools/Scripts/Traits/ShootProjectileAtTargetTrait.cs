using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Shoot Projectile at Target Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Shoot Projectile at Target")]
    public class ShootProjectileAtTargetTrait : Trait
    {
        public override bool Instant => true;
        public override bool RequireId => true;
        public override bool ApplyOnClient => true;

        [SerializeField] private SpriteTrait _spriteTrait;
        [SerializeField] private Trait[] _additionalTraits;
        [SerializeField] private int _moveSpeed = 1;
        [SerializeField] private int _offset = 0;
        [SerializeField] private float _baseRotation = 0f;
        [SerializeField] private bool _rotateProjectile = false;
        [SerializeField] private Trait[] _applyOnWall;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            var owner = _controller.transform.parent.gameObject;
            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = (objOwner) => owner = objOwner;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryOwnerMsg);

            if (owner)
            {
                GameObject target = null;

                var queryTargetMsg = MessageFactory.GenerateQueryTargetMsg();
                queryTargetMsg.DoAfter = (aggroTarget) => target = aggroTarget;
                _controller.gameObject.SendMessageTo(queryTargetMsg, owner);
                MessageFactory.CacheMessage(queryTargetMsg);

                var faceDirection = Vector2Int.zero;

                var queryFacingDirectionMsg = MessageFactory.GenerateQueryFacingDirectionMsg();
                queryFacingDirectionMsg.DoAfter = facing => faceDirection = facing;
                _controller.gameObject.SendMessageTo(queryFacingDirectionMsg, owner);
                MessageFactory.CacheMessage(queryFacingDirectionMsg);

                var direction = target ? (target.transform.position.ToVector2() - _controller.transform.parent.position.ToVector2()).normalized : faceDirection;

                if (direction != Vector2.zero)
                {
                    var offset = direction * _offset;
                    var rotationDirection = _rotateProjectile ? direction.ToZRotation() + _baseRotation : 0f;
                    var projectile = AbilityFactory.Projectile.GenerateUnitWithRotation(_controller.transform.parent.position.ToVector2() + offset, rotationDirection).gameObject;

                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    addTraitToUnitMsg.Trait = _spriteTrait;
                    _controller.gameObject.SendMessageTo(addTraitToUnitMsg, projectile);

                    if (_additionalTraits.Length > 0)
                    {
                        foreach (var trait in _additionalTraits)
                        {
                            addTraitToUnitMsg.Trait = trait;
                            _controller.gameObject.SendMessageTo(addTraitToUnitMsg, projectile);
                        }
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);

                    ObjectManager.RegisterObject(projectile, _worldId);

                    var setupProjectileMsg = MessageFactory.GenerateSetupProjectileMsg();
                    setupProjectileMsg.Direction = direction;
                    setupProjectileMsg.MoveSpeed = _moveSpeed;
                    setupProjectileMsg.ApplyOnWall = _applyOnWall;
                    _controller.gameObject.SendMessageTo(setupProjectileMsg, projectile);
                    MessageFactory.CacheMessage(setupProjectileMsg);

                    var setOwnerMsg = MessageFactory.GenerateSetOwnerMsg();
                    setOwnerMsg.Owner = owner;
                    _controller.gameObject.SendMessageTo(setOwnerMsg, projectile);
                    MessageFactory.CacheMessage(setOwnerMsg);
                }
            }

        }

    }
}