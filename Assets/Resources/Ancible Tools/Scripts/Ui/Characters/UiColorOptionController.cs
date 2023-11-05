using Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Characters
{
    public class UiColorOptionController : MonoBehaviour
    {
        [SerializeField] private ColorOptionType _type;
        [SerializeField] private bool _secondary = false;

        private ColorOption[] _options = new ColorOption[0];
        private Material _material = null;
        private int _currentIndex = 0;

        public void Setup(Material material)
        {
            _material = material;
            _options = ColorOptionFactory.GetOptionsByType(_type);
            _options[_currentIndex].Apply(_material, _secondary);
        }

        public string GetOption()
        {
            return _options[_currentIndex].name;
        }

        public void ApplyIndexChange(int change)
        {
            var index = _currentIndex + change;
            if (index >= _options.Length)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = _options.Length - 1;
            }
            _currentIndex = index;
            _options[_currentIndex].Apply(_material, _secondary);
        }

        void OnDestroy()
        {
            _material = null;
        }

    }
}