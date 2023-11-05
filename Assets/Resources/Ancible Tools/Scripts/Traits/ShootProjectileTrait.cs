using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Shoot Projectile Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Shoot Projectile")]
    public class ShootProjectileTrait : Trait
    {
        public const string PROJECTILE = "Projectile";

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
        [SerializeField] private bool _stopOnWall = true;
        [SerializeField] private Vector2Int _direction = Vector2Int.zero;
        [SerializeField] private bool _registerProjectile = false;
        [SerializeField] private BonusTag[] _tags = new BonusTag[0];

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            var owner = _controller.transform.parent.gameObject;
            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = (objOwner) => owner = objOwner;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryOwnerMsg);

            var available = true;
            if (owner && _registerProjectile)
            {
                available = false;
                var availableProjectileCheckMsg = MessageFactory.GenerateProjectileAvailableCheckMsg();
                availableProjectileCheckMsg.DoAfter = () => available = true;
                availableProjectileCheckMsg.Projectile = name;
                _controller.gameObject.SendMessageTo(availableProjectileCheckMsg, owner);
                MessageFactory.CacheMessage(availableProjectileCheckMsg);
            }

            if (available)
            {
                var direction = _direction;
                if (_direction == Vector2Int.zero)
                {
                    var queryDirectionMsg = MessageFactory.GenerateQueryFacingDirectionMsg();
                    queryDirectionMsg.DoAfter = (objDirection) => direction = objDirection;
                    _controller.gameObject.SendMessageTo(queryDirectionMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(queryDirectionMsg);
                }


                if (direction != Vector2Int.zero)
                {
                    var offset = (direction * _offset).ToVector2(true);
                    var rotationDirection = _rotateProjectile ? direction.ToVector2(false).ToZRotation() + _baseRotation : 0f;
                    var projectile = AbilityFactory.Projectile.GenerateUnitWithRotation(_controller.transform.parent.position.ToVector2() + offset, rotationDirection).gameObject;

                    if (owner && _registerProjectile)
                    {
                        var registerProjectileMsg = MessageFactory.GenerateRegisterProjectileMsg();
                        registerProjectileMsg.MaxStack = MaxStack;
                        registerProjectileMsg.Projectile = projectile;
                        registerProjectileMsg.ProjectileName = name;
                        _controller.gameObject.SendMessageTo(registerProjectileMsg, owner);
                        MessageFactory.CacheMessage(registerProjectileMsg);
                    }

                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    addTraitToUnitMsg.Trait = TraitFactory.Ownership;
                    _controller.gameObject.SendMessageTo(addTraitToUnitMsg, projectile);

                    var setOwnerMsg = MessageFactory.GenerateSetOwnerMsg();
                    setOwnerMsg.Owner = owner;
                    _controller.gameObject.SendMessageTo(setOwnerMsg, projectile);
                    MessageFactory.CacheMessage(setOwnerMsg);

                    
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
                    setupProjectileMsg.StopOnWall = _stopOnWall;
                    setupProjectileMsg.Tags = _tags;
                    if (owner == ObjectManager.Player)
                    {
                        setupProjectileMsg.ReportPosition = true;
                        setupProjectileMsg.WorldId = _worldId;
                    }
                    setupProjectileMsg.Unregister = _registerProjectile;
                    _controller.gameObject.SendMessageTo(setupProjectileMsg, projectile);
                    MessageFactory.CacheMessage(setupProjectileMsg);

                }
            }
            else if (owner && ObjectManager.Player == owner)
            {
                ClientController.SendToServer(new ClientDestroyObjectMessage{TargetId = _worldId});
            }
            
            
        }

        public override string GetDescription()
        {
            var description = base.GetDescription();
            description = string.IsNullOrEmpty(description) ? PROJECTILE : $"{description}{Environment.NewLine}{PROJECTILE}";

            var projectileTraits = _additionalTraits.GetTraitDescriptions();
            foreach (var trait in projectileTraits)
            {
                description = $"{description}{Environment.NewLine}{trait}";
            }

            return description;
        }
    }
}