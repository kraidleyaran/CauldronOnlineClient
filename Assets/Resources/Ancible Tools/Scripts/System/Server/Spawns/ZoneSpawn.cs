using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    [Serializable]
    public class ZoneSpawn
    {
        public ServerUnitTemplate Template;
        public ServerTrait[] AdditionalTraits;
        public Vector2Int Tile;

        public ZoneSpawnData GetData()
        {
            var data = Template.GetData();
            foreach (var trait in AdditionalTraits)
            {
                data.AddTrait(trait.name);
            }

            return new ZoneSpawnData
            {
                Spawn = data,
                Tile = Tile.ToWorldVector(),
                IsWorldPosition = false
            };
        }
    }
}