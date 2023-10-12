using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Loot Chest Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Chests/Loot Chest")]
    public class LootChestServerTrait : ServerTrait
    {
        [SerializeField] private LootTable _lootTable;
        [SerializeField] private SpriteTrait _closedSprite;
        [SerializeField] private SpriteTrait _openSprite;
        [SerializeField] private ServerHitbox _hitbox;
        [SerializeField] private WorldIntRange _drops;
        [SerializeField] private bool _refillChest = false;
        [SerializeField] private WorldIntRange _refillTicks;
        [SerializeField] private bool _destroyOnOpen;
        [SerializeField] private int _destructionTicks = 1;

        public override WorldTraitData GetData()
        {
            return new LootChestTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                ClosedSprite = _closedSprite.name,
                OpenSprite = _openSprite.name,
                Hitbox = _hitbox.GetData(),
                LootTable = _lootTable.name,
                Drops = _drops,
                RefillChest = _refillChest,
                RefillTicks = _refillTicks,
                DestroyAfterOpen =  _destroyOnOpen,
                DestroyTicks = _destructionTicks
            };
        }
    }
}