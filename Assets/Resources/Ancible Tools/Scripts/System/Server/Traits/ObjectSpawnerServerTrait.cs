using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Object Spawner Server Trait", menuName = "Ancible Tools/Server/Traits/Spawning/Object Spawner")]
    public class ObjectSpawnerServerTrait : ServerTrait
    {
        [SerializeField] private ServerUnitTemplate _template;
        [SerializeField] private ServerTrait[] _additionalTraits;
        [SerializeField] private int _maxSpawns = 1;
        [SerializeField] private int _spawnRange = 1;
        [SerializeField] private int _spawnEvery = 1;
        [SerializeField] [Range(0f, 1f)] private float _chanceToSpawn = 1f;
        [SerializeField] private float _bounsOnMissedChance = 0f;

        public override WorldTraitData GetData()
        {
            var objectSpawnData = _template.GetData();
            if (_additionalTraits.Length > 0)
            {
                var traits = objectSpawnData.Traits.ToList();
                traits.AddRange(_additionalTraits.Where(t => t).Select(t => t.name));
                objectSpawnData.Traits = traits.ToArray();
            }

            return new ObjectSpawnerTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                MaxSpawns = _maxSpawns,
                SpawnArea = _spawnRange,
                SpawnEvery = _spawnEvery,
                SpawnData = objectSpawnData,
                ChanceToSpawn = _chanceToSpawn,
                BonusPerMissedChance = _bounsOnMissedChance
            };



        }
    }
}