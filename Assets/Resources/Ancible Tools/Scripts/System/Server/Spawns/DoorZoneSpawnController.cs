using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class DoorZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private bool _startOpen = false;
        [SerializeField] private ItemStack[] _requiredItems = new ItemStack[0];
        [SerializeField] private ServerHitbox _serverHitbox = null;
        [SerializeField] private TriggerEvent[] _triggerEvents = new TriggerEvent[0];
        [SerializeField] private bool _requireAllEvents = false;
        [SerializeField] private bool _allowOpenWithNoItems = false;
        [HideInInspector] public Vector2Int TrappedSpawnPosition = Vector2Int.zero;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new DoorParameter
            {
                Open = _startOpen,
                RequiredItems = _requiredItems.Where(r => r.Item).Select(r => r.GetWorldData()).ToArray(),
                Hitbox = _serverHitbox.GetData(),
                Rotation = transform.localRotation.eulerAngles.z,
                TriggerEvents = _triggerEvents.Where(t => t).Select(e => e.name).ToArray(),
                RequireAllEvents = _requireAllEvents,
                AllowOpenWithNoItems = _allowOpenWithNoItems,
                TrappedSpawnPosition = TrappedSpawnPosition.ToWorldVector()
            });
            return data;
        }

        public override void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            base.RefreshEditorSprite();
            var color = _spriteRenderer.color;
            color.a = _startOpen ? .66f : 1f;
            _spriteRenderer.color = color;
#endif
        }
    }
}