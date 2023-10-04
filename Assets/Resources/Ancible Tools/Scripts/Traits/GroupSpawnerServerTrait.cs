using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Group Spawner Server Trait", menuName = "Ancible Tools/Server/Traits/Spawning/Group Spawner")]
    public class GroupSpawnerServerTrait : ServerTrait
    {
        [SerializeField] private ZoneSpawn[] _objects;
        [SerializeField] private int _spawnEvery = 1;
        [SerializeField] [Range(0f, 1f)] private float _chanceToSpawn = 1f;
        [SerializeField] private float _bonusPerMissedChance = 0f;

        public override WorldTraitData GetData()
        {
            return new GroupSpawnerTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Objects = _objects.Where(o => o.Template).Select(o => o.GetData()).ToArray(),
                SpawnEvery = _spawnEvery,
                ChanceToSpawn = _chanceToSpawn,
                BonusPerMissedChance = _bonusPerMissedChance
            };
        }
    }
}