using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Aspects
{
    public class AspectFactory : MonoBehaviour
    {
        private static AspectFactory _instance = null;

        [SerializeField] private string _internalAspectPath = string.Empty;

        private Dictionary<string, WorldAspect> _aspects = new Dictionary<string, WorldAspect>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _aspects = UnityEngine.Resources.LoadAll<WorldAspect>(_internalAspectPath).ToDictionary(a => a.name, a => a);
            Debug.Log($"Loaded {_aspects.Count} Aspects");
        }

        public static WorldAspect GetAspectByName(string aspectName)
        {
            if (_instance._aspects.TryGetValue(aspectName, out var aspect))
            {
                return aspect;
            }

            return null;
        }
    }
}