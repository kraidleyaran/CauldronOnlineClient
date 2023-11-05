using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class BridgeZoneSpawnController : ZoneSpawnController
    {
        [Header("Bridge Settings")]
        [SerializeField] private TilemapSpriteTrait _sprite;
        [SerializeField] private bool _active = false;
        [SerializeField] private TriggerEvent[] _toggleOnTriggerEvents = new TriggerEvent[0];
        [SerializeField] private RequiredSwitchSignal[] _toggleOnSwitchSignals = new RequiredSwitchSignal[0];

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            var parameter = new BridgeParameter
            {
                Active = _active,
                ToggleOnTriggerEvents = _toggleOnTriggerEvents.Where(t => t).Select(t => t.name).ToArray(),
                ToggleOnSwitchSignals = _toggleOnSwitchSignals.Where(s => s.Switch).Select(s => s.GetData()).ToArray()
            };
            if (_sprite)
            {
                parameter.TilemapSprite = _sprite.name;
                parameter.Size = new WorldVector2Int(_sprite.Tilemap.GridWidth, _sprite.Tilemap.GridHeight);
                
            }
            data.Spawn.AddParameter(parameter);
            return data;
        }
    }
}