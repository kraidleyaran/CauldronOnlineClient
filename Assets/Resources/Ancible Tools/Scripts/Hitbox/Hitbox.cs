using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Hitbox
{
    [CreateAssetMenu(fileName = "Hitbox", menuName = "Ancible Tools/Physics/Hitbox")]
    public class Hitbox : ScriptableObject
    {
        public HitboxController Controller;
    }
}