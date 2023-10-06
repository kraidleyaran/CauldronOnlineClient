using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class LootChestZoneSpawnController : ZoneSpawnController
    {
        [Header("Loot Settings")]
        [SerializeField] private LootTable _lootTable;
        [SerializeField] private WorldIntRange _drops;
        [SerializeField] private SpriteTrait _closedSprite;
        [SerializeField] private SpriteTrait _openSprite;
        [SerializeField] private ServerHitbox _hitbox;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new LootChestParameter
            {
                LootTable = _lootTable.name,
                Drops = _drops,
                ClosedSprite = _closedSprite.name,
                OpenSprite = _openSprite.name,
                Open = false,
                Hitbox = _hitbox.GetData()
            });
            return data;
        }

        public override void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            if (_closedSprite && _spriteRenderer && _closedSprite.Sprite != _spriteRenderer.sprite)
            {
                _spriteRenderer.sprite = _closedSprite.Sprite;
                _spriteRenderer.transform.SetLocalScaling(_closedSprite.Scaling);
                _spriteRenderer.transform.SetLocalPosition(_closedSprite.Offset);
            }
#endif
        }
    }
}