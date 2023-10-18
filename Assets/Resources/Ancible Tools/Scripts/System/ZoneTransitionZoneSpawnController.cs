using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ZoneTransitionZoneSpawnController : ZoneSpawnController
    {
        [Header("Zone Transition Settings")]
        public WorldZone Zone;
        [HideInInspector] public Vector2Int Position;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            var rotation = transform.localRotation.eulerAngles;
            data.Spawn.AddParameter(new ZoneTransitionParameter
            {
                Zone = Zone.name,
                Position = Position.ToWorldVector(),
                Rotation = rotation.z
            });

            return data;
        }
    }
}