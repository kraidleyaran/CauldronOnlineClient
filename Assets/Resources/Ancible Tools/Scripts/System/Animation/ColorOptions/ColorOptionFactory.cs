using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions
{
    public class ColorOptionFactory : MonoBehaviour
    {
        private static ColorOptionFactory _instance = null;

        [SerializeField] private string[] _colorOptionPaths = new string[0];

        private Dictionary<string, ColorOption> _colorOptions = new Dictionary<string, ColorOption>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            foreach (var path in _colorOptionPaths)
            {
                var options = UnityEngine.Resources.LoadAll<ColorOption>(path);
                foreach (var option in options)
                {
                    if (!_colorOptions.ContainsKey(option.name))
                    {
                        _colorOptions.Add(option.name, option);
                    }
                }
            }

            Debug.Log($"Loaded {_colorOptions.Count} Color Options");
        }

        public static ColorOption[] GetOptionsByType(ColorOptionType type)
        {
            return _instance._colorOptions.Values.Where(t => t.Type == type).ToArray();
        }

        public static ColorOption GetOptionByName(string name)
        {
            if (_instance._colorOptions.TryGetValue(name, out var option))
            {
                return option;
            }

            return null;
        }


    }
}