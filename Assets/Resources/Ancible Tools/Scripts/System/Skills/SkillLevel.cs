using System;
using Assets.Resources.Ancible_Tools.Scripts.Traits;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [Serializable]
    public class SkillLevel
    {
        public int RequiredLevel;
        public Trait[] ApplyOnLevel = new Trait[0];
    }
}