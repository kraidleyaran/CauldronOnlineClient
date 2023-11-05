using Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class DelayedZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private int _delayTicks = 0;

        [Header("Delayed Zone Editor References")]
        [SerializeField] private ServerUnitTemplate _emptyUnit;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var emptyUnit = _emptyUnit.GetData();
            emptyUnit.AddParameter(new DelayedSpawnParameter { DelayTicks = _delayTicks, Spawn = base.GetData(tile)});
            var data = new ZoneSpawnData
            {
                Tile = tile,
                ShowAppearance = false,
                StartActive = true,
                Spawn = emptyUnit
            };
            return data;
        }
    }
}