using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ObjectManager : MonoBehaviour
    {
        public static GameObject Player { get; private set; }
        public static string PlayerObjectId { get; private set; }
        public static SpriteTrait DefaultPlayerSprite => _instance._defaultPlayerSprite;
        public static CombatStats StartingStats => _instance._startingCombatStats;
        public static string LocalFilter => _instance._localFilter;

        private static ObjectManager _instance = null;

        [SerializeField] private UnitTemplate _playerTemplate;
        [SerializeField] private CombatStats _startingCombatStats;
        [SerializeField] private SpriteTrait _defaultPlayerSprite;
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

            _instance._setLoadoutMsg.Loadout = data.Loadout;
            Player.SendMessageTo(_instance._setLoadoutMsg, Player);

            Player.transform.SetParent(_instance.transform);
        }

        public static void GenerateNetworkObject(ClientObjectData data)
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

                                _instance._setupDoorMsg.Open = door.Open;
                                _instance._setupDoorMsg.RequiredItems = door.RequiredItems;
                                _instance._setupDoorMsg.Rotation = door.Rotation;
                                _instance._setupDoorMsg.Hitbox = door.Hitbox;
                                _instance._setupDoorMsg.AllowOpenWithNoItems = door.AllowOpenWithNoItems;
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

                                _instance._setupSwitchMsg.Switch = switchParam.Name;
                                _instance._setupSwitchMsg.Hitbox = switchParam.Hitbox;
                                _instance._setupSwitchMsg.Signals = switchParam.Signals;
                                _instance._setupSwitchMsg.CurrentSignal = switchParam.CurrentSignal;
                                obj.SendMessageTo(_instance._setupSwitchMsg, obj);
                            }
                            break;
                        case LootChestParameter.TYPE:
                            if (parameter is LootChestParameter lootChest)
                            {
                                addTraitToUnitMsg.Trait = TraitFactory.NetworkChest;
                                obj.SendMessageTo(addTraitToUnitMsg, obj);

                                _instance._setupChestMsg.OpenSprite = lootChest.OpenSprite;
                                _instance._setupChestMsg.CloseSprite = lootChest.ClosedSprite;
                                _instance._setupChestMsg.Open = lootChest.Open;
                                obj.SendMessageTo(_instance._setupChestMsg, obj);
                            }
                            break;
                    }
                }

                if (isMonster)
                {
                    _instance.gameObject.SendMessageTo(IsMonsterMessage.INSTANCE, obj);
                }

                MessageFactory.CacheMessage(addTraitToUnitMsg);

                _instance._networkObjects.Add(data.Id, obj);
                _instance._networkReverseLookup.Add(obj, data.Id);

                obj.transform.SetParent(_instance.transform);
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
            }
        }
    }
}