﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Combat;
using DG.Tweening;
using FileDataLib;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class DataController : MonoBehaviour
    {
        public static WorldState WorldState { get; private set; }
        public static float Interpolation => _instance._interpoliation;
        public static Vector2 TrueZero => new Vector2(Interpolation / 2f, Interpolation / 2f);
        public static WorldCharacterData CurrentCharacter => _instance._currentCharacter;

        private static DataController _instance = null;

        private static UpdateWorldStateMessage _updateWorldStateMsg = new UpdateWorldStateMessage();

        [SerializeField] private int _framesPerSecond = 60;
        [SerializeField] private bool _permanent = false;
        [SerializeField] private float _interpoliation = .3f;
        [SerializeField] private string _characterFolderName;
        [SerializeField] private int _saveEverySecond = 300;
        [SerializeField] private WorldZone _startingZone;

        private string _characterFolderPath = string.Empty;

        private WorldCharacterData _currentCharacter = null;
        private WorldClientState _clientState = WorldClientState.Disconnected;
        private Sequence _saveTimer = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            //In order to force a frame count, we have to set the vSyncCount to 0 or it will ignore the targetFrameRate varaible;
            QualitySettings.vSyncCount = 0;
            //This forces unity to make sure that FixedUpdate ticks are equally distributed within a given Update tick always instead of sometimes being less or more
            Time.fixedDeltaTime = 1f / _framesPerSecond;
            Application.targetFrameRate = _framesPerSecond;
            if (_permanent)
            {
                DontDestroyOnLoad(gameObject);
            }
            _characterFolderPath = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{_characterFolderName}";
            if (!Directory.Exists(_characterFolderPath))
            {
                Directory.CreateDirectory(_characterFolderPath);
            }
            SubscribeToMessages();
            
        }

        public static void CheckPlayerSaveFiles()
        {
            var files = Directory.GetFiles(_instance._characterFolderPath, $"*.{WorldCharacterData.EXTENSION}");
            if (files.Length > 0)
            {
                var file = files[0];
                var result = FileData.LoadData<WorldCharacterData>(file);
                _instance._currentCharacter = result.Success ? result.Data : _instance.GenerateNewCharacter();
            }
            else
            {
                _instance._currentCharacter = _instance.GenerateNewCharacter();
            }
        }

        public static void SavePlayerData()
        {
            var worldCharacterData = _instance._currentCharacter;
            var player = ObjectManager.Player;
            var queryInventoryMsg = MessageFactory.GenerateQueryInventoryMsg();
            queryInventoryMsg.DoAfter = items => worldCharacterData.Inventory = items.Select(i => i.GetData()).ToArray();
            _instance.gameObject.SendMessageTo(queryInventoryMsg, player);
            MessageFactory.CacheMessage(queryInventoryMsg);

            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, vitals, bonusSecondary) =>
            {
                worldCharacterData.Stats = baseStats;
                worldCharacterData.Vitals = vitals;
            };
            _instance.gameObject.SendMessageTo(queryCombatStatsMsg, player);
            MessageFactory.CacheMessage(queryCombatStatsMsg);

            var queryAspectsMsg = MessageFactory.GenerateQueryAspectsMsg();
            queryAspectsMsg.DoAfter = (aspects, availablepoints) =>
            {
                worldCharacterData.Aspects = aspects.Select(a => a.GetData()).ToArray();
                worldCharacterData.AvailablePoints = availablepoints;
            };
            _instance.gameObject.SendMessageTo(queryAspectsMsg, player);
            MessageFactory.CacheMessage(queryAspectsMsg);

            var loadout = new LoadoutSlot[0];

            var queryLoadoutMsg = MessageFactory.GenerateQueryLoadoutMsg();
            queryLoadoutMsg.DoAfter = slots => loadout = slots;
            _instance.gameObject.SendMessageTo(queryLoadoutMsg, player);
            MessageFactory.CacheMessage(queryLoadoutMsg);

            var saveSlots = new List<LoadoutSlotData>();
            for (var i = 0; i < loadout.Length; i++)
            {
                var slot = loadout[i];
                if (!slot.IsEmpty)
                {
                    saveSlots.Add(slot.GetData(i));
                }
            }

            worldCharacterData.Loadout = saveSlots.ToArray();

            var queryExperienceMsg = MessageFactory.GenerateQueryExperienceMsg();
            queryExperienceMsg.DoAfter = (level, experience, nextLevel) =>
            {
                worldCharacterData.Level = level;
                worldCharacterData.Experience = experience;
            };
            _instance.gameObject.SendMessageTo(queryExperienceMsg, player);
            MessageFactory.CacheMessage(queryExperienceMsg);

            var queryArmorMsg = MessageFactory.GenerateQueryArmorEquipmentMsg();
            queryArmorMsg.DoAfter = equipped => worldCharacterData.EquippedArmor = equipped.Select(e => e.Item.name).ToArray();
            _instance.gameObject.SendMessageTo(queryArmorMsg, player);
            MessageFactory.CacheMessage(queryArmorMsg);
            var originPath = $"{_instance._characterFolderPath}{Path.DirectorySeparatorChar}{worldCharacterData.Name}.{WorldCharacterData.EXTENSION}";
            if (File.Exists(originPath))
            {
                File.Delete(originPath);
            }
            
            var path = $"{_instance._characterFolderPath}{Path.DirectorySeparatorChar}{worldCharacterData.Name}.{WorldCharacterData.EXTENSION}";
            var result = FileData.SaveData(path, worldCharacterData);
            if (result.Success)
            {
                Debug.Log("Character Data Saved Succesfully");
            }
            else
            {
                Debug.LogWarning($"Error while saving character data to path {path} - {(result.HasException ? $"{result.Exception}" : "Unknown Error" )} ");
            }
        }

        public static void SetWorldState(WorldState state)
        {
            if (WorldState != state)
            {
                WorldState = state;
                _updateWorldStateMsg.State = WorldState;
                _instance.gameObject.SendMessage(_updateWorldStateMsg);
            }

            switch (WorldState)
            {
                case WorldState.Inactive:
                    if (_instance._saveTimer != null)
                    {
                        if (_instance._saveTimer.IsActive())
                        {
                            _instance._saveTimer.Kill();
                        }

                        _instance._saveTimer = null;
                    }
                    break;
                case WorldState.Loading:
                case WorldState.Active:
                    if (_instance._saveTimer == null)
                    {
                        _instance._saveTimer = DOTween.Sequence().AppendInterval(_instance._saveEverySecond).OnComplete(_instance.SaveTimerFinished);
                    }
                    else
                    {
                        _instance._saveTimer.Restart();
                    }
                    break;
            }
        }

        private void SaveTimerFinished()
        {
            SavePlayerData();
            _saveTimer = DOTween.Sequence().AppendInterval(_saveEverySecond).OnComplete(SaveTimerFinished);
        }

        private WorldCharacterData GenerateNewCharacter()
        {
            return new WorldCharacterData
            {
                Name = "Default",
                Aspects = new AspectData[0],
                AvailablePoints = 0,
                EquippedArmor = new string[0],
                Experience = 0,
                Level = 0,
                Inventory = new ItemStackData[0],
                Stats = ObjectManager.StartingStats,
                Vitals =
                    new CombatVitals
                    {
                        Health = ObjectManager.StartingStats.Health,
                        Mana = ObjectManager.StartingStats.Mana
                    },
                Loadout = new LoadoutSlotData[0],
                Zone = _startingZone.name,
                Position = _startingZone.DefaultSpawn.ToWorldVector()
            };
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateClientStateMessage>(UpdateClientState);
        }

        private void UpdateClientState(UpdateClientStateMessage msg)
        {
            if (msg.State == WorldClientState.Disconnected && _clientState == WorldClientState.Connected)
            {
                SavePlayerData();
                ObjectManager.ClearObjects();
                WorldZoneManager.Clear();
            }

            _clientState = msg.State;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}