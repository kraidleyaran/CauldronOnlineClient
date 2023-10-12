using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using Battlehub.Dispatcher;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ClientController : MonoBehaviour
    {
        public static WorldClientState State => _instance._client.State;
        public static ClientSettings Settings => _instance._settings;

        private static ClientController _instance = null;

        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 42069;
        [SerializeField] private int _checkMessagesEveryMs = 10;
        [SerializeField] private string _settingsFileName = "clientsettings.json";
        [SerializeField] private int _maxPingResults = 100;

        private WorldClient _client = null;

        private UpdateClientStateMessage _updateClientStateMsg = new UpdateClientStateMessage();
        private SetWorldPositionMessage _setWorldPositionMsg = new SetWorldPositionMessage();

        private Thread _messagingThread = null;
        private bool _active = false;
        private string _clientId = string.Empty;

        private ClientSettings _settings = null;

        private Dictionary<string, List<ClientMultiPartMessage>> _multiPartMessages = new Dictionary<string, List<ClientMultiPartMessage>>();

        private List<int> _pingResults = new List<int>();
        private DateTime _lastPingStamp = DateTime.MinValue;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            var path = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{_settingsFileName}";
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    _settings = JsonConvert.DeserializeObject<ClientSettings>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Client settings not found - {ex}");
                    Debug.LogWarning("Loading default client settings");
                    _settings = new ClientSettings {IpAddres = _ipAddress, Port = _port};
                    File.AppendAllText(path, JsonConvert.SerializeObject(_settings));
                }
            }
            else
            {
                _settings = new ClientSettings { IpAddres = _ipAddress, Port = _port };
                File.AppendAllText(path, JsonConvert.SerializeObject(_settings));
            }
            _client = new WorldClient();
            _client.OnStateUpdate += StateUpdate;

            SubscribeToMessages();
        }

        void Start()
        {

        }

        public static void Connect()
        {
            if (_instance._client.State == WorldClientState.Disconnected)
            {
                _instance._active = true;
                _instance._messagingThread = new Thread(_instance.CheckMessages);
                _instance._messagingThread.Start();
                _instance._client.Connect(_instance._settings.IpAddres, _instance._settings.Port);
            }
        }

        public static void Disconnect()
        {
            if (_instance._client.State != WorldClientState.Disconnected)
            {
                _instance._client.Disconnect();
            }
        }

        public static void SendToServer<T>(T clientMsg) where T : ClientMessage
        {
            if (_instance._client.State != WorldClientState.Disconnected)
            {
                clientMsg.Sender = null;
                clientMsg.ClientId = _instance._clientId;
                _instance._client.Send(clientMsg);
            }
        }

        public static int GetAveragePing()
        {
            if (_instance._pingResults.Count > 0)
            {
                var ping = 0;
                foreach (var result in _instance._pingResults)
                {
                    ping += result;
                }

                return ping / _instance._pingResults.Count;
            }

            return 0;

        }

        public static void TransferPlayer(WorldZone zone, WorldVector2Int pos)
        {
            DataController.SetWorldState(WorldState.Loading);
            DataController.SavePlayerData();
            var clientData = DataController.CurrentCharacter.ToClientData();
            clientData.Sprite = ObjectManager.DefaultPlayerSprite.name;
            SendToServer(new ClientZoneTransferRequestMessage{Data = clientData, Zone = zone.name, Position = pos});
        }

        public static void RespawnPlayer()
        {
            var clientData = DataController.CurrentCharacter.ToClientData();
            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            var combined = new CombatStats();
            queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, vitals, bonusSecondary) => { combined = baseStats + bonusStats; };
            clientData.Stats = combined;
            clientData.Vitals = new CombatVitals {Health = combined.Health, Mana = combined.Mana};
            clientData.Sprite = ObjectManager.DefaultPlayerSprite.name;
            _instance.gameObject.SendMessageTo(queryCombatStatsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryCombatStatsMsg);

            SendToServer(new ClientRespawnRequestMessage{Data = clientData, Zone = DataController.CurrentCharacter.Zone, Position = DataController.CurrentCharacter.Position});
        }

        

        public static void SetConnctionSettings(string ipAddress, int port)
        {
            _instance._settings.IpAddres = ipAddress;
            _instance._settings.Port = port;
            var path = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{_instance._settingsFileName}";
            File.WriteAllText(path, JsonConvert.SerializeObject(_instance._settings));
        }

        private void StateUpdate(WorldClientState state)
        {
            _active = state != WorldClientState.Disconnected;
            Dispatcher.Current.BeginInvoke(() =>
            {
                _updateClientStateMsg.State = state;
                gameObject.SendMessage(_updateClientStateMsg);
                Debug.Log($"Client State - {state}");
            });
        }

        private void CheckMessages()
        {
            while (_active)
            {
                var messages = _client.ReadMessages();
                if (messages.Length > 0)
                {
                    foreach (var msg in messages)
                    {
                        if (msg.MessageId == ClientMultiPartMessage.ID && msg is ClientMultiPartMessage mulitPart)
                        {
                            if (!_multiPartMessages.ContainsKey(mulitPart.MultiPartId))
                            {
                                _multiPartMessages.Add(mulitPart.MultiPartId, new List<ClientMultiPartMessage>());
                            }
                            _multiPartMessages[mulitPart.MultiPartId].Add(mulitPart);
                            if (_multiPartMessages[mulitPart.MultiPartId].Count >= mulitPart.TotalParts)
                            {
                                var message = _multiPartMessages[mulitPart.MultiPartId].ToArray().ToClientMessage();
                                _multiPartMessages.Remove(mulitPart.MultiPartId);
                                Dispatcher.Current.BeginInvoke(() =>
                                {
                                    gameObject.SendMessage(message);
                                });
                            }
                        }



                        Dispatcher.Current.BeginInvoke(() =>
                        {
                            gameObject.SendMessage(msg);
                        });
                    }


                }

                Thread.Sleep(_checkMessagesEveryMs + Mathf.RoundToInt(TickController.WorldTick));
            }

            _messagingThread = null;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClientConnectResultMessage>(ClientConnectionResult);
            gameObject.Subscribe<ClientCreateCharacterResultMessage>(ClientCharacterResult);
            gameObject.Subscribe<ClientWorldSettingsResultMessage>(ClientWorldSettingsResult);
            gameObject.Subscribe<ClientPingMessage>(ClientPing);
            gameObject.Subscribe<ClientZoneTransferResultMessage>(ClientZoneTransferResult);
            gameObject.Subscribe<ClientRespawnResultMessage>(ClientRespawnResult);
            //gameObject.Subscribe<ClientMultiPartMessage>(ClientMultiPart);
        }

        private void ClientConnectionResult(ClientConnectResultMessage msg)
        {
            if (msg.Success)
            {
                DataController.SetWorldState(WorldState.Loading);
                _clientId = msg.ClientId;
                Debug.Log($"Connected to Server - ClientId:{msg.ClientId} - Sending Settings Request");
                _lastPingStamp = DateTime.UtcNow;
                SendToServer(new ClientPingMessage());
                SendToServer(new ClientWorldSettingsRequestMessage());
            }
            else
            {
                Debug.LogWarning($"Unable to connect to server - {msg.Message}");
                Disconnect();
            }
        }

        private void ClientCharacterResult(ClientCreateCharacterResultMessage msg)
        {
            if (msg.Success)
            {
                WorldZoneManager.LoadZone(msg.Zone);
                ObjectManager.GeneratePlayerObject(DataController.CurrentCharacter, msg.ObjectId, msg.Position);
                Debug.Log("Character request received - Sending Object request");
                SendToServer(new ClientObjectRequestMessage());
            }
        }

        private void ClientWorldSettingsResult(ClientWorldSettingsResultMessage msg)
        {
            Debug.Log("Settings received - Sending character request");
            var clientData = DataController.CurrentCharacter.ToClientData();
            clientData.Sprite = ObjectManager.DefaultPlayerSprite.name;
            SendToServer(new ClientCreateCharacterRequestMessage{Data = clientData, Zone = DataController.CurrentCharacter.Zone, Position = DataController.CurrentCharacter.Position});
        }

        private void ClientPing(ClientPingMessage msg)
        {
            var latency = (int)(DateTime.UtcNow - _lastPingStamp).TotalMilliseconds;
            _lastPingStamp = DateTime.UtcNow;
            _pingResults.Add(latency);
            while (_pingResults.Count > _maxPingResults)
            {
                _pingResults.RemoveAt(0);
            }
            SendToServer(msg);
        }

        private void ClientZoneTransferResult(ClientZoneTransferResultMessage msg)
        {
            if (msg.Success)
            {
                if (msg.Zone != WorldZoneManager.CurrentZone.name)
                {
                    ObjectManager.ClearObjects();
                    ObjectManager.SetPlayerObjectId(msg.ObjectId);
                    WorldZoneManager.LoadZone(msg.Zone);
                    _setWorldPositionMsg.Position = msg.Position;
                    gameObject.SendMessageTo(_setWorldPositionMsg, ObjectManager.Player);
                    CameraController.SetPosition(msg.Position.ToWorldVector(), true);
                    SendToServer(new ClientObjectRequestMessage());
                }
                else
                {
                    _setWorldPositionMsg.Position = msg.Position;
                    gameObject.SendMessageTo(_setWorldPositionMsg, ObjectManager.Player);
                    CameraController.SetPosition(msg.Position.ToWorldVector(), true);

                    DataController.SetWorldState(WorldState.Active);

                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Active;
                    gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(setUnitStateMsg);
                }

            }
            else
            {
                Debug.LogWarning($"Error while transferring zones - {msg.Message}");
            }
        }

        private void ClientRespawnResult(ClientRespawnResultMessage msg)
        {
            if (msg.Success)
            {
                var changeZone = !WorldZoneManager.IsCurrentZone(msg.Zone);
                if (changeZone)
                {
                    ObjectManager.ClearObjects();
                    ObjectManager.SetPlayerObjectId(msg.ObjectId);
                    WorldZoneManager.LoadZone(msg.Zone);
                    SendToServer(new ClientObjectRequestMessage());
                }

                _setWorldPositionMsg.Position = msg.Position;
                gameObject.SendMessageTo(_setWorldPositionMsg, ObjectManager.Player);

                gameObject.SendMessageTo(FullHealMessage.INSTANCE, ObjectManager.Player);

                CameraController.SetPosition(msg.Position.ToWorldVector(), true);

                if (!changeZone)
                {
                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                    setUnitStateMsg.State = UnitState.Active;
                    gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(setUnitStateMsg);
                }
                
            }
        }

        //private void ClientMultiPart(ClientMultiPartMessage msg)
        //{

        //}

        void OnDestroy()
        {
            _active = false;
            if (_client.State != WorldClientState.Disconnected)
            {
                _client.Disconnect();
            }

            _active = false;
        }

    }
}