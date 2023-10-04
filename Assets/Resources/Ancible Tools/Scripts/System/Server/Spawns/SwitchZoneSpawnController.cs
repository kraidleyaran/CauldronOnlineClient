using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Switches;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class SwitchZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private SwitchSignal _switch;
        [SerializeField] private HitboxData _hitbox;
        [SerializeField] private SpriteTrait[] _signals;
        [SerializeField] private int _startingSignal;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new SwitchParameter
            {
                Name = _switch.name,
                Signals = _signals.Where(s => s).Select(s => s.name).ToArray(),
                CurrentSignal = _startingSignal,
                Hitbox = _hitbox
            });
            return data;
        }

        public override void RefreshEditorSprite()
        {
            if (_signals.Length > 0 && _spriteRenderer)
            {
                var signal = _startingSignal < _signals.Length ? _startingSignal : 0;
                var sprite = _signals[signal];
                if (sprite)
                {
                    _spriteRenderer.sprite = sprite.Sprite;
                    _spriteRenderer.transform.SetLocalScaling(sprite.Scaling);
                    _spriteRenderer.transform.SetLocalPosition(sprite.Offset);
                }

            }
        }
    }
}