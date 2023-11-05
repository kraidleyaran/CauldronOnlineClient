using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Movable Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Movable")]
    public class MovableServerTrait : ServerTrait
    {
        [SerializeField] private int _moveSpeed = 1;
        [SerializeField] private ServerHitbox _interactHitbox;
        [SerializeField] private ServerHitbox _horizontalInteractHitbox;
        [SerializeField] private WorldOffset _offset = new WorldOffset();

        public override WorldTraitData GetData()
        {
            return new MovableTraitData
            {
                Name = name,
                MaxStack = MaxStack,
                Hitbox = _interactHitbox.GetData(),
                HorizontalHitbox = _horizontalInteractHitbox.GetData(),
                MoveSpeed = _moveSpeed,
                Offset = _offset
            };
        }
    }
}