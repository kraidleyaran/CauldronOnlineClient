using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class KeyItemChestSpawnController : ZoneSpawnController
    {
        [SerializeField] private KeyItem _keyItem;
        [SerializeField] private int _stack = 1;
        [SerializeField] private SpriteTrait _closedSprite;
        [SerializeField] private SpriteTrait _openSprite;
        [SerializeField] private ServerHitbox _hitbox;
        [SerializeField] private ItemStack[] _applyToPlayers = new ItemStack[0];
        [SerializeField] private TriggerEvent[] _applyTriggerEventOnChestOpen;

        [Header("Key Item Chest Internal References")]
        [SerializeField] private SpriteRenderer _itemRenderer;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new KeyItemChestParameter
            {
                Item = new WorldItemStackData { Item = _keyItem.name, Stack = _stack},
                ClosedSprite = _closedSprite.name,
                OpenSprite = _openSprite.name,
                Hitbox = _hitbox.GetData(),
                RewardToPlayers = _applyToPlayers.Where(i => i.Item).Select(i => i.GetWorldData()).ToArray(),
                ApplyEventsOnOpen = _applyTriggerEventOnChestOpen.Where(t => t).Select(t => t.name).ToArray()
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

            if (_keyItem && _itemRenderer)
            {
                _itemRenderer.sprite = _keyItem.Sprite.Sprite;
                _itemRenderer.transform.SetLocalScaling(_keyItem.Sprite.Scaling);
            }
#endif
        }
    }
}