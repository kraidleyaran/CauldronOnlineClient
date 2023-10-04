using System.Linq;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class TriggerHitboxZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private TriggerEvent[] _triggerEvents = new TriggerEvent[0];

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            var hitboxData = new HitboxData
            {
                Size = new WorldVector2Int((int) transform.localScale.x, (int) transform.localScale.y),
                Offset = WorldVector2Int.Zero
            };

            data.Spawn.AddParameter(new TriggerEventHitboxParameter{Hitbox = hitboxData, TriggerEvents = _triggerEvents.Where(t => t).Select(t => t.name).ToArray()});

            return data;
        }
    }
}