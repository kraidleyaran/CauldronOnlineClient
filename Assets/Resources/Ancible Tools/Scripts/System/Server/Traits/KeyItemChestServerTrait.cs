using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Key Item Chest Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Chests/Key Item Chest")]
    public class KeyItemChestServerTrait : ServerTrait
    {
        [SerializeField] private KeyItem _item;
        [SerializeField] private int _stack = 1;
        [SerializeField] private SpriteTrait _closedSprite;
        [SerializeField] private SpriteTrait _openSprite;
        [SerializeField] private ServerHitbox _hitbox;

        public override WorldTraitData GetData()
        {
            return new KeyItemChestTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Item = new WorldItemStackData {Item = _item.name, Stack = _stack},
                ClosedSprite = _closedSprite.name,
                OpenSprite = _openSprite.name,
                Hitbox = _hitbox.GetData()
            };
        }
    }
}