using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class SpriteLayerFactory : MonoBehaviour
    {
        public static SpriteLayer Unit => _instance._unit;
        public static SpriteLayer Fx => _instance._fx;

        private static SpriteLayerFactory _instance = null;

        [SerializeField] private SpriteLayer _unit = null;
        [SerializeField] private SpriteLayer _fx = null;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        
    }
}