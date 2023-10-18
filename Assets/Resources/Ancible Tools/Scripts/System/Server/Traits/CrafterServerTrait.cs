using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Crafter Server Trait", menuName = "Ancible Tools/Server/Traits/Items/Crafter")]
    public class CrafterServerTrait : ServerTrait
    {
        [SerializeField] private ItemRecipe[] _recipes = new ItemRecipe[0];
        [SerializeField] private ServerHitbox _hitbox = null;

        public override WorldTraitData GetData()
        {
            return new CrafterTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Recipes = _recipes.Where(r => r.Item).Select(r => r.GetData()).ToArray(),
                Hitbox = _hitbox.GetData()
            };
        }
    }
}