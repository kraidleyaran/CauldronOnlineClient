using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Hitbox
{
    [CreateAssetMenu(fileName = "Collision Layer", menuName = "Ancible Tools/Physics/Collision Layer")]
    public class CollisionLayer : ScriptableObject
    {
        public string LayerName;

        public int ToLayer()
        {
            return LayerMask.NameToLayer(LayerName);
        }

        public int ToMask()
        {
            return 1 << ToLayer();
        }
    }
}