using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Dev
{
    public class UiDevWarpController : MonoBehaviour
    {
        [SerializeField] private Dropdown _zoneDropdown;
        [SerializeField] private Dropdown _positionsDropdown;
        [SerializeField] private Button _warpButton = null;

        private Dictionary<string, Vector2Int> _positions = new Dictionary<string, Vector2Int>();
        private Dictionary<int, string> _positionIndexLookup = new Dictionary<int, string>();
        private Dictionary<string, WorldZone> _zones = new Dictionary<string, WorldZone>();
        private Dictionary<int, string> _indexLookup = new Dictionary<int, string>();

        void Awake()
        {
            var zones = WorldZoneManager.GetAllZones().OrderBy(z => z.DisplayName).ToArray();
            var index = 0;
            var setIndex = 0;
            foreach (var zone in zones)
            {
                _zones.Add(zone.DisplayName, zone);
                _indexLookup.Add(index, zone.DisplayName);
                if (zone == WorldZoneManager.CurrentZone)
                {
                    setIndex = index;
                }

                index++;
            }

            var ordered = _indexLookup.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
            _zoneDropdown.AddOptions(ordered);
            _zoneDropdown.SetValueWithoutNotify(setIndex);
            ZoneChanged();
            SubscribeToMessages();
        }

        public void ZoneChanged()
        {
            if (_indexLookup.TryGetValue(_zoneDropdown.value, out var zoneName) && _zones.TryGetValue(zoneName, out var zone))
            {
                _positions.Clear();
                _positionIndexLookup.Clear();
                _positionsDropdown.ClearOptions();
                var positions = zone.Controller.GetPlayerSpawns().OrderBy(s => s.DisplayName);
                var positionIndex = 0;
                foreach (var position in positions)
                {
                    _positions.Add(position.DisplayName, position.WorldPosition);
                    _positionIndexLookup.Add(positionIndex, position.DisplayName);
                    positionIndex++;
                }

                var ordered = _positionIndexLookup.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
                _positionsDropdown.AddOptions(ordered);
                _positionsDropdown.value = 0;
            }
        }

        public void Confirm()
        {
            if (DataController.WorldState == WorldState.Active)
            {
                if (_indexLookup.TryGetValue(_zoneDropdown.value, out var zoneName) && _zones.TryGetValue(zoneName, out var zone))
                {
                    if (_positionIndexLookup.TryGetValue(_positionsDropdown.value, out var positionName) && _positions.TryGetValue(positionName, out var position))
                    {
                        if (zone != WorldZoneManager.CurrentZone)
                        {
                            ClientController.TransferPlayer(zone, position.ToWorldVector());
                        }
                        else
                        {
                            var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                            setWorldPositionMsg.Position = position.ToWorldVector();
                            gameObject.SendMessageTo(setWorldPositionMsg, ObjectManager.Player);
                            MessageFactory.CacheMessage(setWorldPositionMsg);
                        }
                    }
                }
            }
            
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            _warpButton.interactable = msg.State == WorldState.Active;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}