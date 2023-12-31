﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    public class TraitFactory : MonoBehaviour
    {
        public static NetworkAggroTrait NetworkAggro => _instance._defaultAggroTrait;
        public static NetworkHurtboxTrait DefaultHurtbox => _instance._defaultHurtboxTrait;
        public static CombatStatsTrait DefaultCombatStats => _instance._defaultCombatStatsTrait;
        public static NetworkHitboxTrait DefaultNetworkHitboxTrait => _instance._networkHitboxTrait;
        public static OnUnitDeathTrait NetworkOnUnitDeath => _instance._networkOnUnitDeathTrait;
        public static NetworkShopTrait NetworkShop => _instance._networkShopTrait;
        public static TerrainTrait NetworkTerrain => _instance._networkTerrainTrait;
        public static DialogueTrait NetworkDialogue => _instance._networkDialogueTrait;
        public static DoorTrait NetworkDoor => _instance._networkDoorTrait;
        public static NetworkTriggerHitboxTrait NetworkTriggerHitbox => _instance._networkTriggerHitboxTrait;
        public static NetworkAbilityManagerTrait NetworkAbilityManager => _instance._networkAbilityManagerTrait;
        public static KnockbackReceiverTrait NetworkKnockbackReceiver => _instance._networkKnockbackReceiverTrait;
        public static SwitchTrait NetworkSwitch => _instance._networkSwitchTrait;
        public static ChestTrait NetworkChest => _instance._networkChestTrait;
        public static WorldPositionTrait WorldPosition => _instance._worldPositionTrait;
        public static NameTagTrait NameTag => _instance._nameTagTrait;
        public static VisualFxTrait AppearanceFx => _instance._appearanceVisualFxTrait;
        public static ZoneTransitionTrait ZoneTransition => _instance._zoneTransitionTrait;
        public static NetworkCrafterTrait Crafter => _instance._networkCrafterTrait;
        public static BridgeTrait Bridge => _instance._bridgeTrait;
        public static OwnershipTrait Ownership => _instance._ownershipTrait;
        public static WalledTrait NetworkWalled => _instance._networkWalledTrait;
        public static NetworkMovementTrait NetworkMovement => _instance._networkMovementTrait;
        public static MovableTrait Movable => _instance._movableTrait;
        public static NetworkMovableHelperTrait NetworkMovableHelper => _instance._networkMovableHelperTrait;
        public static HitboxTrait NetworkPlayerTerrain => _instance._networkPlayerTerrainTrait;
        public static ProjectileManagerTrait ProjectileManager => _instance._projectileManagerTrait;
        public static NetworkRollingTrait NetworkRolling => _instance._networkRollingTrait;
        public static ProjectileRedirectTrait ProjectileRedirect => _instance._projectileRedirectTrait;
        public static BombableDoorTrait BombableDoor => _instance._bombableDoorTrait;

        private static TraitFactory _instance = null;

        [SerializeField] private string[] _internalTraitPaths = new string[0];
        [SerializeField] private NetworkAggroTrait _defaultAggroTrait = null;
        [SerializeField] private NetworkHurtboxTrait _defaultHurtboxTrait = null;
        [SerializeField] private CombatStatsTrait _defaultCombatStatsTrait = null;
        [SerializeField] private NetworkHitboxTrait _networkHitboxTrait = null;
        [SerializeField] private OnUnitDeathTrait _networkOnUnitDeathTrait = null;
        [SerializeField] private NetworkShopTrait _networkShopTrait;
        [SerializeField] private TerrainTrait _networkTerrainTrait;
        [SerializeField] private DialogueTrait _networkDialogueTrait;
        [SerializeField] private DoorTrait _networkDoorTrait;
        [SerializeField] private NetworkTriggerHitboxTrait _networkTriggerHitboxTrait;
        [SerializeField] private NetworkAbilityManagerTrait _networkAbilityManagerTrait;
        [SerializeField] private KnockbackReceiverTrait _networkKnockbackReceiverTrait;
        [SerializeField] private SwitchTrait _networkSwitchTrait;
        [SerializeField] private ChestTrait _networkChestTrait;
        [SerializeField] private WorldPositionTrait _worldPositionTrait;
        [SerializeField] private NameTagTrait _nameTagTrait;
        [SerializeField] private VisualFxTrait _appearanceVisualFxTrait;
        [SerializeField] private ZoneTransitionTrait _zoneTransitionTrait;
        [SerializeField] private NetworkCrafterTrait _networkCrafterTrait;
        [SerializeField] private BridgeTrait _bridgeTrait;
        [SerializeField] private OwnershipTrait _ownershipTrait;
        [SerializeField] private WalledTrait _networkWalledTrait;
        [SerializeField] private NetworkMovementTrait _networkMovementTrait;
        [SerializeField] private MovableTrait _movableTrait;
        [SerializeField] private NetworkMovableHelperTrait _networkMovableHelperTrait;
        [SerializeField] private HitboxTrait _networkPlayerTerrainTrait;
        [SerializeField] private ProjectileManagerTrait _projectileManagerTrait;
        [SerializeField] private NetworkRollingTrait _networkRollingTrait;
        [SerializeField] private ProjectileRedirectTrait _projectileRedirectTrait;
        [SerializeField] private BombableDoorTrait _bombableDoorTrait;

        private Dictionary<string, Trait> _traits = new Dictionary<string, Trait>();
        private Dictionary<string, SpriteTrait> _sprites = new Dictionary<string, SpriteTrait>();
        private Dictionary<string, TilemapSpriteTrait> _tilemapSprites = new Dictionary<string, TilemapSpriteTrait>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            foreach (var path in _internalTraitPaths)
            {
                var traits = UnityEngine.Resources.LoadAll<Trait>(path);
                foreach (var trait in traits)
                {
                    if (!_traits.ContainsKey(trait.name))
                    {
                        _traits.Add(trait.name, trait);
                        if (trait is SpriteTrait sprite)
                        {
                            _sprites.Add(sprite.name, sprite);
                        }
                        else if (trait is TilemapSpriteTrait tilemap)
                        {
                            _tilemapSprites.Add(tilemap.name, tilemap);
                        }
                    }
                }
            }
        }

        public static SpriteTrait GetSprite(string spriteName)
        {
            if (_instance._sprites.TryGetValue(spriteName, out var sprite))
            {
                return sprite;
            }

            return null;
        }

        public static TilemapSpriteTrait GetTilemapSprite(string tilemap)
        {
            if (_instance._tilemapSprites.TryGetValue(tilemap, out var tilemapSprite))
            {
                return tilemapSprite;
            }

            return null;
        }

        public static Trait GetTraitByName(string traitName)
        {
            if (_instance._traits.TryGetValue(traitName, out var trait))
            {
                return trait;
            }

            return null;
        }

        public static Trait[] GetTraitsByName(string[] traitNames)
        {
            var returnTraits = new List<Trait>();
            foreach (var traitName in traitNames)
            {
                if (_instance._traits.TryGetValue(traitName, out var trait))
                {
                    returnTraits.Add(trait);
                }
            }

            return returnTraits.ToArray();
        }
    }
}