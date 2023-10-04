using System;
using Assets.Resources.Ancible_Tools.Scripts.Traits;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    [Serializable]
    public class AbilityStep
    {
        public int Frames = 5;
        public AbilityState State;
        public Trait[] ApplyToOwner = new Trait[0];
        public Trait[] ApplyToAttack = new Trait[0];
    }
}