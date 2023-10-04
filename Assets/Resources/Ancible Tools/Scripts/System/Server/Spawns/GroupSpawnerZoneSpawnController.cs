using System.Linq;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class GroupSpawnerZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private ZoneSpawn[] _objects;
        [SerializeField] private int _spawnEvery = 1;
        [SerializeField] [Range(0f,1f)] private float _chanceToSpawn = 1f;
        [SerializeField] private float _bonusPerMissedChance = 0f;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new GroupSpawnParameter
            {
                Objects = _objects.Where(o => o.Template).Select(o => o.GetData()).ToArray(),
                SpawnEvery = _spawnEvery,
                ChanceToSpawn = _chanceToSpawn,
                BonusOnMissedChance = _bonusPerMissedChance
            });
            return data;
        }
    }
}