using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Zones
{
    public class WorldZoneManager : MonoBehaviour
    {
        public static WorldZone CurrentZone => _instance._currentZone;
        public static WorldZoneController CurrentZoneController => _instance._controller;

        private static WorldZoneManager _instance = null;

        [SerializeField] private string _internalZoneFolderPath = string.Empty;

        private Dictionary<string, WorldZone> _zones = new Dictionary<string, WorldZone>();

        private WorldZone _currentZone = null;
        private WorldZoneController _controller = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _zones = UnityEngine.Resources.LoadAll<WorldZone>(_internalZoneFolderPath).ToDictionary(z => z.name, z => z);
            Debug.Log($"Loaded {_zones.Count} World Zones");
        }

        public static bool LoadZone(string zoneName)
        {
            if (_instance._zones.TryGetValue(zoneName, out var worldZone))
            {
                if (!_instance._currentZone || _instance._currentZone != worldZone)
                {
                    if (_instance._controller)
                    {
                        Destroy(_instance._controller.gameObject);
                        WorldEventManager.SetActiveState(false);
                    }

                    _instance._currentZone = worldZone;
                    _instance._controller = Instantiate(_instance._currentZone.Controller);
                    WorldEventManager.SetActiveState(true);
                    return true;
                }

                Debug.LogWarning("Zone already loaded");
                return false;
            }
            Debug.LogWarning($"Could not find zone {zoneName}");
            return false;
        }

        public static bool IsCurrentZone(string zoneName)
        {
            return _instance._currentZone && _instance._currentZone.name == zoneName;
        }

        public static WorldZone[] GetAllZones()
        {
            return _instance._zones.Values.ToArray();
        }

        public static WorldZone GetZoneByName(string name)
        {
            if (_instance._zones.TryGetValue(name, out var zone))
            {
                return zone;
            }

            return null;
        }

        public static void Clear()
        {
            if (_instance._controller)
            {
                Destroy(_instance._controller.gameObject);
                _instance._currentZone = null;
            }
        }
    }
}