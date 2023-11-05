using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using Assets.Resources.Ancible_Tools.Scripts.Ui.FloatingText;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ObjectManager : MonoBehaviour
    {
        public static GameObject Player { get; private set; }
        public static string PlayerObjectId { get; private set; }
        public static SpriteTrait DefaultPlayerSprite => _instance._defaultPlayerSprite;
        public static CombatStats StartingStats => _instance._startingCombatStats;
        public static PremadeItemSlot[] StartingItems => _instance._startingEquippedActionItems;
        public static PremadeAbilitySlot[] StartingAbility => _instance._startingAbilities;
        public static string LocalFilter => _instance._localFilter;
        

        private static ObjectManager _instance = null;

        [SerializeField] private UnitTemplate _playerTemplate;
        [SerializeField] private CombatStats _startingCombatStats;
        [SerializeField] private SpriteTrait _defaultPlayerSprite;
        [SerializeField] private RecolorTrait _defaultRecolorTrait;
        [SerializeField] private PremadeItemSlot[] _startingEquippedActionItems = new PremadeItemSlot[0];
        [SerializeField] private PremadeAbilitySlot[] _startingAbilities = new PremadeAbilitySlot[0];
        [SerializeField] private UnitTemplate _networkObjectTemplate;
        [SerializeField] private string _localFilter = "Local";
        [SerializeField] private bool _showPlayerClone = false;
        

        private Dictionary<string, GameObject> _networkObjects = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, string> _networkReverseLookup = new Dictionary<GameObject, string>();

        private SetAggroRangeMessage _setAggroRangeMsg = new SetAggroRangeMessage();
        private SetCombatStatsMessage _setCombatStatsMsg = new SetCombatStatsMessage();
        private SetHurtboxSizeMessage _setHurtBoxSizeMsg = new SetHurtboxSizeMessage();
        private SetupHitboxesMessage _setupHitboxesMsg = new SetupHitboxesMessage();
        private SetupKnockbackMessage _setupKnockbackMsg = new SetupKnockbackMessage();
        private SetApplyAbilityMessage _setApplyAbilityMsg = new SetApplyAbilityMessage();
        private SetupShopMessage _setupShopMsg = new SetupShopMessage();
        private SetupTerrainMessage _setupTerrainMsg = new SetupTerrainMessage();
        private SetupDoorMessage _setupDoorMsg = new SetupDoorMessage();
        private SetupNetworkTriggerHitboxMessage _setupNetworkTriggerHitboxMsg = new SetupNetworkTriggerHitboxMessage();
        private SetExperienceMessage _setExperienceMsg = new SetExperienceMessage();
        private SetLoadedAspectsMessage _setLoadedAspectsMsg = new SetLoadedAspectsMessage();
        private SetArmorEquipmentMessage _setArmorEquipmentMsg = new SetArmorEquipmentMessage();
        private SetLoadoutMessage _setLoadoutMsg = new SetLoadoutMessage();
        private SetupSwitchMessage _setupSwitchMsg = new SetupSwitchMessage();
        private SetupChestMessage _setupChestMsg = new SetupChestMessage();
        private SetInventoryMessage _setInventoryMsg = new SetInventoryMessage();
        private SetNameTagMessage _setNameTagMsg = new SetNameTagMessage();
        private SetupZoneTransitionMessage _setupZoneTransitionMsg = new SetupZoneTransitionMessage();
        private SetupCrafterMessage _setupCrafterMsg = new SetupCrafterMessage();
        private SetupBridgeMessage _setupBridgeMsg = new SetupBridgeMessage();
        private SetSpriteColorDataMessage _setSpriteColorDataMsg = new SetSpriteColorDataMessage();
        private SetupMovableMessage _setupMovableMsg = new SetupMovableMessage();
        private SetupWalledMessage _setupWalledMsg = new SetupWalledMessage();
        private SetupProjectileRedirectMessage _setupProjectileRedirectMsg = new SetupProjectileRedirectMessage();
        private SetSkillsMessage _setSkillsMsg = new SetSkillsMessage();
        private SetupBombableDoorMessage _setupBombableDoorMsg = new SetupBombableDoorMessage();

        private bool _editorMode = false;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

#if UNITY_EDITOR
            _editorMode = true;
#endif
            _instance = this;
            SubscribeToMessages();
            DataController.CheckPlayerSaveFiles();
        }

        public static void GeneratePlayerObject(WorldCharacterData data, string objectId, WorldVector2Int pos)
        {
            PlayerObjectId = objectId;
            var position = pos.ToWorldVector();
            Player = _instance._playerTemplate.GenerateUnit(position).gameObject;
            Player.name = "Player";

            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

            addTraitToUnitMsg.Trait = _instance._defaultPlayerSprite;
            Player.SendMessageTo(addTraitToUnitMsg, Player);

            _instance._setSpriteColorDataMsg.Data = data.Colors;
            Player.SendMessageTo(_instance._setSpriteColorDataMsg, Player);

            //addTraitToUnitMsg.Trait = _instance._defaultRecolorTrait;
            //Player.SendMessageTo(addTraitToUnitMsg, Player);

            MessageFactory.CacheMessage(addTraitToUnitMsg);

            _instance._setCombatStatsMsg.Stats = data.Stats;
            _instance._setCombatStatsMsg.Vitals = data.Vitals;
            _instance._setCombatStatsMsg.Report = true;
            Player.SendMessageTo(_instance._setCombatStatsMsg, Player);

            _instance._setLoadedAspectsMsg.Aspects = data.Aspects;
            _instance._setLoadedAspectsMsg.AvailablePoints = data.AvailablePoints;
            Player.SendMessageTo(_instance._setLoadedAspectsMsg, Player);

            _instance._setExperienceMsg.Experience = data.Experience;
            _instance._setExperienceMsg.Level = data.Level;
            Player.SendMessageTo(_instance._setExperienceMsg, Player);

            _instance._setArmorEquipmentMsg.Equipped = data.EquippedArmor;
            Player.SendMessageTo(_instance._setArmorEquipmentMsg, Player);

            _instance._setInventoryMsg.Items = data.Inventory;
            _instance._setInventoryMsg.Gold = data.Gold;
            Player.SendMessageTo(_instance._setInventoryMsg, Player);

            _instance._setLoadedAspectsMsg.Aspects = data.Aspects;
            _instance._setLoadedAspectsMsg.AvailablePoints = data.AvailablePoints;
            Player.SendMessageTo(_instance._setLoadedAspectsMsg, Player);

            if (data.Skills == null)
            {
                data.Skills = new SkillData[0];
            }
            _instance._setSkillsMsg.Skills = data.Skills;
            Player.SendMessageTo(_instance._setSkillsMsg, Player);

            _instance._setLoadoutMsg.Loadout = data.Loadout;
            Player.SendMessageTo(_instance._setLoadoutMsg, Player);

            

            Player.transform.SetParent(_instance.transform);
        }

        public static void GenerateNetworkObject(ClientObjectData data, bool showAppearance = false)
        {
            if (!_instance._networkObjects.ContainsKey(data.Id) && (PlayerObjectId != data.Id || _instance._editorMode && _instance._showPlayerClone))
            {
                var obj = _instance._networkObjectTemplate.GenerateUnit(data.Position.ToWorldVector()).gameObject;

                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

                if (!string.IsNullOrEmpty(data.Sprite))
                {
                    var sprite = TraitFactory.GetSprite(data.Sprite);
                    if (sprite)
                    {
                        addTraitToUnitMsg.Trait = sprite;
                        obj.SendMessageTo(addTraitToUnitMsg, obj);
                    }
                }

                if (data.ShowName)
                {
                    addTraitToUnitMsg.Trait = TraitFactory.NameTag;
                    obj.SendMessageTo(addTraitToUnitMsg, obj);

                    _instance._setNameTagMsg.Name = data.DisplayName;
                    obj.SendMessageTo(_instance._setNameTagMsg, obj);
                }

                var isMonster = false;


                foreach (var paraData in data.Parameters)
                {
                    var parameter = paraData.GenerateObjectParameter();
                    switch (parameter.Type)
                    {
                        case AggroParameter.TYPE:
                            if (parameter is AggroParameter aggro)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkAggro;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setAggroRangeMsg.AggroRange = aggro.AggroRange;
                                obj.SendMessageTo(_instance._setAggroRangeMsg, obj);
                            }
                            break;
                        case HurtboxParameter.TYPE:
                            if (parameter is HurtboxParameter hurtbox)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.DefaultHurtbox;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setHurtBoxSizeMsg.Size = hurtbox.Size;
                                _instance._setHurtBoxSizeMsg.Offset = hurtbox.Offset;
                                _instance._setHurtBoxSizeMsg.Knockback = hurtbox.Knockback;
                                obj.SendMessageTo(_instance._setHurtBoxSizeMsg, obj);
                            }
                            break;
                        case CombatStatsParameter.TYPE:
                            if (parameter is CombatStatsParameter combatStats)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.DefaultCombatStats;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setCombatStatsMsg.Stats = combatStats.Stats;
                                _instance._setCombatStatsMsg.Vitals = combatStats.Vitals;
                                _instance._setCombatStatsMsg.BonusSecondary = combatStats.BonusSecondary;
                                _instance._setCombatStatsMsg.Report = combatStats.Monster;
                                isMonster = combatStats.Monster;
                                obj.SendMessageTo(_instance._setCombatStatsMsg, obj);
                            }
                            break;
                        case HitboxParameter.TYPE:
                            if (parameter is HitboxParameter hitbox)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.DefaultNetworkHitboxTrait;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupHitboxesMsg.Hitboxes = hitbox.Hitboxes;
                                obj.SendMessageTo(_instance._setupHitboxesMsg, obj);
                            }
                            break;
                        case KnockbackReceiverParameter.TYPE:
                            if (parameter is KnockbackReceiverParameter knockback)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkKnockbackReceiver;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupKnockbackMsg.ReceiveKnockback = knockback.ReceiveKnockback;
                                obj.SendMessageTo(_instance._setupKnockbackMsg, obj);
                            }
                            break;
                        case AbilityManagerParameter.TYPE:
                            if (parameter is AbilityManagerParameter ability)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkAbilityManager;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.ProjectileManager;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setApplyAbilityMsg.ApplyAbility = ability.ApplyTraitsOnClient;
                                obj.SendMessageTo(_instance._setApplyAbilityMsg, obj);
                            }

                            break;
                        case ObjectDeathParameter.TYPE:
                            addTraitToUnitMsg.Trait = TraitFactory.NetworkOnUnitDeath;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);
                            break;
                        case ObjectPathParameter.TYPE:
                            if (parameter is ObjectPathParameter path && path.Positions.Length > 0)
                            {
                                var setPathMsg = MessageFactory.GenerateSetPathMsg();
                                setPathMsg.MoveSpeed = path.Speed;
                                setPathMsg.Path = path.Positions;
                                obj.SendMessageTo(setPathMsg, obj);
                                MessageFactory.CacheMessage(setPathMsg);
                            }
                            break;
                        case ShopParameter.TYPE:
                            if (parameter is ShopParameter shop)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.NetworkShop;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupShopMsg.Items = shop.Items;
                                _instance._setupShopMsg.Hitbox = shop.Hitbox;
                                obj.SendMessageTo(_instance._setupShopMsg, obj);
                            }
                            break;
                        case TerrainParameter.TYPE:
                            if (parameter is TerrainParameter terrain)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkTerrain;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupTerrainMsg.Hitbox = terrain.Hitbox;
                                _instance._setupTerrainMsg.IsGround = terrain.IsGround;
                                obj.SendMessageTo(_instance._setupTerrainMsg, obj);
                            }
                            break;
                        case DialogueParameter.TYPE:
                            if (parameter is DialogueParameter dialogue)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkDialogue;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                var setDialogueMsg = MessageFactory.GenerateSetDialogueMsg();
                                setDialogueMsg.Dialogue = dialogue.Dialogue;
                                setDialogueMsg.ActionText = dialogue.ActionText;
                                obj.SendMessageTo(setDialogueMsg, obj);
                                MessageFactory.CacheMessage(setDialogueMsg);
                            }
                            break;
                        case DoorParameter.TYPE:
                            if (parameter is DoorParameter door)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkDoor;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupDoorMsg.Open = door.Open;
                                _instance._setupDoorMsg.RequiredItems = door.RequiredItems;
                                _instance._setupDoorMsg.Rotation = door.Rotation;
                                _instance._setupDoorMsg.Hitbox = door.Hitbox;
                                _instance._setupDoorMsg.AllowOpenWithNoItems = door.AllowOpenWithNoItems;
                                _instance._setupDoorMsg.TrappedSpawnPosition = door.TrappedSpawnPosition;
                                obj.SendMessageTo(_instance._setupDoorMsg, obj);
                            }
                            break;
                        case TriggerEventHitboxParameter.TYPE:
                            if (parameter is TriggerEventHitboxParameter triggerEvent)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkTriggerHitbox;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupNetworkTriggerHitboxMsg.Hitbox = triggerEvent.Hitbox;
                                _instance._setupNetworkTriggerHitboxMsg.TriggerEvents = triggerEvent.TriggerEvents;
                                obj.SendMessageTo(_instance._setupNetworkTriggerHitboxMsg, obj);
                            }
                            break;
                        case SwitchParameter.TYPE:
                            if (parameter is SwitchParameter switchParam)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkSwitch;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupSwitchMsg.Switch = switchParam.Name;
                                _instance._setupSwitchMsg.Hitbox = switchParam.Hitbox;
                                _instance._setupSwitchMsg.Signals = switchParam.Signals;
                                _instance._setupSwitchMsg.CurrentSignal = switchParam.CurrentSignal;
                                _instance._setupSwitchMsg.CombatInteractable = switchParam.CombatInteractable;
                                _instance._setupSwitchMsg.Locked = switchParam.Locked;
                                _instance._setupSwitchMsg.LockOnInteract = switchParam.LockOnInteract;
                                obj.SendMessageTo(_instance._setupSwitchMsg, obj);
                            }
                            break;
                        case LootChestParameter.TYPE:
                            if (parameter is LootChestParameter lootChest)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.NetworkChest;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupChestMsg.OpenSprite = lootChest.OpenSprite;
                                _instance._setupChestMsg.CloseSprite = lootChest.ClosedSprite;
                                _instance._setupChestMsg.Open = lootChest.Open;
                                _instance._setupChestMsg.Hitbox = lootChest.Hitbox;
                                obj.SendMessageTo(_instance._setupChestMsg, obj);
                            }
                            break;
                        case KeyItemChestParameter.TYPE:
                            if (parameter is KeyItemChestParameter keyItemChest)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.NetworkChest;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupChestMsg.OpenSprite = keyItemChest.OpenSprite;
                                _instance._setupChestMsg.CloseSprite = keyItemChest.ClosedSprite;
                                _instance._setupChestMsg.Open = keyItemChest.Open;
                                _instance._setupChestMsg.Hitbox = keyItemChest.Hitbox;
                                obj.SendMessageTo(_instance._setupChestMsg, obj);
                            }
                            break;
                        case ZoneTransitionParameter.TYPE:
                            if (parameter is ZoneTransitionParameter zoneTransition)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.ZoneTransition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupZoneTransitionMsg.Zone = zoneTransition.Zone;
                                _instance._setupZoneTransitionMsg.Position = zoneTransition.Position;
                                _instance._setupZoneTransitionMsg.Rotation = zoneTransition.Rotation;
                                obj.SendMessageTo(_instance._setupZoneTransitionMsg, obj);
                            }
                            break;
                        case CrafterParameter.TYPE:
                            if (parameter is CrafterParameter crafter)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.Crafter;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupCrafterMsg.Recipes = crafter.Recipes;
                                _instance._setupCrafterMsg.Hitbox = crafter.Hitbox;
                                obj.SendMessageTo(_instance._setupCrafterMsg, obj);
                            }
                            break;
                        case BridgeParameter.TYPE:
                            if (parameter is BridgeParameter bridge)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.Bridge;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupBridgeMsg.TilemapSprite = bridge.TilemapSprite;
                                _instance._setupBridgeMsg.Size = bridge.Size;
                                _instance._setupBridgeMsg.Active = bridge.Active;
                                obj.SendMessageTo(_instance._setupBridgeMsg, obj);
                            }
                            break;
                        case PlayerParameter.TYPE:
                            if (parameter is PlayerParameter player)
                            {
                                _instance._setSpriteColorDataMsg.Data = player.Colors;
                                obj.SendMessageTo(_instance._setSpriteColorDataMsg, obj);
                            }

                            addTraitToUnitMsg.Trait = TraitFactory.NetworkPlayerTerrain;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);

                            addTraitToUnitMsg.Trait = TraitFactory.NetworkMovableHelper;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);

                            addTraitToUnitMsg.Trait = TraitFactory.NetworkMovement;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);

                            addTraitToUnitMsg.Trait = TraitFactory.NetworkAbilityManager;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);

                            addTraitToUnitMsg.Trait = TraitFactory.ProjectileManager;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);

                            addTraitToUnitMsg.Trait = TraitFactory.NetworkRolling;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);

                            _instance._setApplyAbilityMsg.ApplyAbility = false;
                            obj.SendMessageTo(_instance._setApplyAbilityMsg, obj);

                            break;
                        case WalledParameter.TYPE:
                            if (parameter is WalledParameter walled)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkWalled;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupWalledMsg.Hitbox = walled.Hitbox;
                                _instance._setupWalledMsg.CheckForPlayer = walled.CheckForPlayer;
                                _instance._setupWalledMsg.IgnoreGround = walled.IgnoreGround;
                                obj.SendMessageTo(_instance._setupWalledMsg, obj);
                            }
                            break;
                        case MovementParameter.TYPE:
                            addTraitToUnitMsg.Trait = TraitFactory.NetworkMovement;
                            obj.SendMessageTo(addTraitToUnitMsg, obj);
                            break;
                        case MovableParameter.TYPE:
                            if (parameter is MovableParameter movable)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.WorldPosition;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                addTraitToUnitMsg.Trait = TraitFactory.Movable;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupMovableMsg.Hitbox = movable.Hitbox;
                                _instance._setupMovableMsg.MoveSpeed = movable.MoveSpeed;
                                _instance._setupMovableMsg.Horizontal = movable.HorizontalHitbox;
                                _instance._setupMovableMsg.Offset = movable.Offset;
                                obj.SendMessageTo(_instance._setupMovableMsg, obj);
                            }
                            break;
                        case ProjectileRedirectParameter.TYPE:
                            if (parameter is ProjectileRedirectParameter projectileRedirect)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.ProjectileRedirect;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupProjectileRedirectMsg.Direction = projectileRedirect.Direction;
                                _instance._setupProjectileRedirectMsg.Hitbox = projectileRedirect.Hitbox;
                                _instance._setupProjectileRedirectMsg.Tags = projectileRedirect.Tags;
                                obj.SendMessageTo(_instance._setupProjectileRedirectMsg, obj);
                            }
                            break;
                        case BombableDoorParameter.TYPE:
                            if (parameter is BombableDoorParameter bombable)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.BombableDoor;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupBombableDoorMsg.Hitbox = bombable.Hitbox;
                                _instance._setupBombableDoorMsg.Open = bombable.Open;
                                _instance._setupBombableDoorMsg.BombableExperience = bombable.BombingExperience;
                                obj.SendMessageTo(_instance._setupBombableDoorMsg, obj);
                            }
                            break;
                    }
                }

                if (isMonster)
                {
                    _instance.gameObject.SendMessageTo(IsMonsterMessage.INSTANCE, obj);
                }

                _instance._networkObjects.Add(data.Id, obj);
                _instance._networkReverseLookup.Add(obj, data.Id);

                var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                setWorldPositionMsg.Position = data.Position;
                obj.SendMessageTo(setWorldPositionMsg, obj);
                MessageFactory.CacheMessage(setWorldPositionMsg);

                obj.transform.SetParent(_instance.transform);

                if (showAppearance)
                {
                    addTraitToUnitMsg.Trait = TraitFactory.AppearanceFx;
                    obj.SendMessageTo(addTraitToUnitMsg, obj);
                }

                if (!data.Active)
                {
                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Disabled;
                    obj.SendMessageTo(setUnitStateMsg, obj);
                    MessageFactory.CacheMessage(setUnitStateMsg);
                }

                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
            else
            {
                Debug.Log($"Duplicate Object Id Detected - {data.Id}");
            }

        }

        public static GameObject GetObjectById(string id)
        {
            if (_instance._networkObjects.TryGetValue(id, out var obj))
            {
                return obj;
            }

            return null;
        }

        public static string GetId(GameObject obj)
        {
            if (_instance._networkReverseLookup.TryGetValue(obj, out var id))
            {
                return id;
            }

            return string.Empty;
        }

        public static void DestroyNetworkObject(GameObject obj)
        {
            if (_instance._networkReverseLookup.TryGetValue(obj, out var id))
            {
                _instance._networkReverseLookup.Remove(obj);
                _instance._networkObjects.Remove(id);
            }
            Destroy(obj);
        }

        public static void RegisterObject(GameObject obj, string id = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                id = $"{_instance._localFilter}-{Guid.NewGuid().ToString()}";
            }
            if (!_instance._networkObjects.ContainsKey(id))
            {
                _instance._networkObjects.Add(id, obj);
                _instance._networkReverseLookup.Add(obj, id);
            }
        }

        public static void ClearObjects(bool clearPlayer = false)
        {
            UiFloatingTextManager.Clear();
            var objs = _instance._networkObjects.Values.ToArray();
            foreach (var obj in objs)
            {
                Destroy(obj);
            }
            _instance._networkObjects.Clear();
            _instance._networkReverseLookup.Clear();

            if (clearPlayer)
            {
                Destroy(Player);
                Player = null;
                PlayerObjectId = string.Empty;
            }
        }

        public static void SetPlayerObjectId(string objId)
        {
            PlayerObjectId = objId;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClientObjectResultMessage>(ClientObjectResult);
        }

        private void ClientObjectResult(ClientObjectResultMessage msg)
        {
            Debug.Log($"Client Object Result received - Success:{msg.Success}");
            UiServerStatusWindow.SetStatusText("Objects received - Building world");
            if (msg.Success)
            {
                if (_networkObjectTemplate)
                {
                    foreach (var data in msg.Data)
                    {
                        GenerateNetworkObject(data);
                    }
                }

                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Active;
                gameObject.SendMessageTo(setUnitStateMsg, Player);
                MessageFactory.CacheMessage(setUnitStateMsg);

                DataController.SetWorldState(WorldState.Active);
                UiServerStatusWindow.Clear();
            }
        }
    }
}