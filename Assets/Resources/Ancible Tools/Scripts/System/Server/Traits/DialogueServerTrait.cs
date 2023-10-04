using CauldronOnlineCommon.Data.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Dialogue Server Trait", menuName = "Ancible Tools/Server/Traits/Interactables/Dialogue")]
    public class DialogueServerTrait : ServerTrait
    {
        [SerializeField] private string[] _dialogue = new string[0];
        [SerializeField] private ServerHitbox _hitbox;

        public override WorldTraitData GetData()
        {
            return new DialogueTraitData {Dialogue = _dialogue, Name = name, MaxStack = MaxStack, Hitbox = _hitbox.GetData()};
        }
    }
}