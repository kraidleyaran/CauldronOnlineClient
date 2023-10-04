using Assets.Resources.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    [CreateAssetMenu(fileName = "Attack Setup", menuName = "Ancible Tools/Abilities/Attack Setup")]
    public class AttackSetup : ScriptableObject
    {
        [SerializeField] private AttackStep _backSwing;
        [SerializeField] private AttackStep _startup;
        [SerializeField] private AttackStep _active;
        [SerializeField] private AttackStep _recovery;
        public SwingAnimationTrait SwingAnimation;

        public AttackStep GetAttackStepByAbilityState(AbilityState state)
        {
            switch (state)
            {
                case AbilityState.Backswing:
                    return _backSwing;
                case AbilityState.Startup:
                    return _startup;
                case AbilityState.Active:
                    return _active;
                case AbilityState.Recovery:
                    return _recovery;
                default:
                    return _active;
            }
        }
    }
}