using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    public class ShaderFxFactory : MonoBehaviour
    {
        public static FlashColorShaderFxController FlashColor => _instance._flashColorTemplate;

        private static ShaderFxFactory _instance = null;

        [SerializeField] private FlashColorShaderFxController _flashColorTemplate;

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