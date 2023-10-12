﻿using Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Spawn Object Server Trait", menuName = "Ancible Tools/Server/Traits/General/Spawn Object")]
    public class SpawnObjectServerTrait : ServerTrait
    {
        [SerializeField] private ZoneSpawn _spawn;

        public override WorldTraitData GetData()
        {
            return new SpawnObjectTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Spawn = _spawn.GetData(),
            };
        }
    }
}