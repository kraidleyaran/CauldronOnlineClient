using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class BridgeZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private TilemapSpriteTrait _sprite;
        [SerializeField] private bool _startActive = false;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            var parameter = new BridgeParameter();
            if (_sprite)
            {
                parameter.TilemapSprite = _sprite.name;
                parameter.Size = new WorldVector2Int(_sprite.Tilemap.GridWidth, _sprite.Tilemap.GridHeight);
            }
            parameter.Active = _startActive;
            data.Spawn.AddParameter(parameter);
            return data;
        }
    }
}