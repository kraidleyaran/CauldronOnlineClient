using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    public class AbilityFactory : MonoBehaviour
    {
        public static UnitTemplate Ability => _instance._abilityTemplate;
        public static UnitTemplate Projectile => _instance._projectileTemplate;

        private static AbilityFactory _instance = null;

        [SerializeField] private string[] _internalAbilityPaths = new string[0];
        [SerializeField] private UnitTemplate _abilityTemplate;
        [SerializeField] private UnitTemplate _projectileTemplate;

        private Dictionary<string, WorldAbility> _abilities = new Dictionary<string, WorldAbility>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            foreach (var path in _internalAbilityPaths)
            {
                var abilities = UnityEngine.Resources.LoadAll<WorldAbility>(path);
                foreach (var ability in abilities)
                {
                    if (!_abilities.ContainsKey(ability.name))
                    {
                        _abilities.Add(ability.name, ability);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate ability detected - {ability.name}");
                    }
                }
            }
            Debug.Log($"Loaded {_abilities.Count} Abilities");
        }

        public static WorldAbility GetAbilityByName(string abilityName)
        {
            if (_instance._abilities.TryGetValue(abilityName, out var ability))
            {
                return ability;
            }

            return null;
        }

    }
}