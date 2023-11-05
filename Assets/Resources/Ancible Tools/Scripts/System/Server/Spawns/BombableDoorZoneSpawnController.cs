using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class BombableDoorZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private ServerHitbox _hitbox;
        [SerializeField] private int _bombableExperience = 25;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new BombableDoorParameter
            {
                Hitbox = _hitbox.GetData(),
                Open = false,
                BombingExperience = _bombableExperience
            });
            return data;
        }
    }
}