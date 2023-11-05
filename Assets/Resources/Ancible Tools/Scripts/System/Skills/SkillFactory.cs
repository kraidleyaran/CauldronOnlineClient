using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    public class SkillFactory : MonoBehaviour
    {
        public static WorldSkill Bombing => _instance._bombing;

        private static SkillFactory _instance = null;

        [SerializeField] private string _skillsPath = string.Empty;
        [SerializeField] private WorldSkill _bombing;

        private Dictionary<string, WorldSkill> _skills = new Dictionary<string, WorldSkill>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _skills = UnityEngine.Resources.LoadAll<WorldSkill>(_skillsPath).ToDictionary(s => s.name, s => s);
        }

        public static WorldSkill GetSkillByName(string skillName)
        {
            if (_instance._skills.TryGetValue(skillName, out var skill))
            {
                return skill;
            }

            return null;
        }
    }
}