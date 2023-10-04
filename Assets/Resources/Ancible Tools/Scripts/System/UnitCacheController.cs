using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class UnitCacheController : MonoBehaviour
    {
        private static UnitCacheController _instance = null;

        private static List<CachedUnit> _cachedUnits = new List<CachedUnit>();
        private static List<CachedUnit> _usedUnits = new List<CachedUnit>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public static void CacheUnit(string name, UnitController controller)
        {
            var cachedUnit = _usedUnits.QuerySingle(c => c.Name == name).Value;
            if (cachedUnit != null)
            {
                _usedUnits.Remove(cachedUnit);
                cachedUnit.Controller = controller;
            }
            else
            {
                cachedUnit = new CachedUnit { Name = name, Controller = controller };
            }
            controller.transform.SetParent(_instance.transform);
            controller.transform.localPosition = Vector3.zero;
            controller.transform.rotation = Quaternion.Euler(0, 0, 0);
            controller.gameObject.SetActive(false);
            _cachedUnits.Add(cachedUnit);
            //_disableUnits.Add(cachedUnit);

        }

        public static UnitController GetCachedUnit(string name)
        {
            var cachedUnit = _cachedUnits.QuerySingle(u => u.Name == name && u.Controller);
            if (cachedUnit.HasValue)
            {
                var controller = cachedUnit.Value.Controller;
                controller.gameObject.SetActive(true);
                cachedUnit.Value.Controller = null;
                _usedUnits.Add(cachedUnit.Value);
                return controller;
            }
            else
            {
                return null;
            }
        }
    }
}