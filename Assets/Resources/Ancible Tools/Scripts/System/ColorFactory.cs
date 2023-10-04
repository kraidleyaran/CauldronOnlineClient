using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ColorFactory : MonoBehaviour
    {
        public static Color PositiveStat => _instance._positiveStat;
        public static Color NegativeStat => _instance._negativeStat;

        private static ColorFactory _instance = null;

        [SerializeField] private Color _positiveStat = Color.green;
        [SerializeField] private Color _negativeStat = Color.red;

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