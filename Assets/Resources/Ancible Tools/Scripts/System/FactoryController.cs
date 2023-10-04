using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class FactoryController : MonoBehaviour
    {
        public static UnitController UNIT_CONTROLLER => _instance._unitControllerTemplate;
        public static TraitController TRAIT_CONTROLLER => _instance._traitControllerTemplate;
        public static SpriteController SPRITE_CONTROLLER => _instance._spriteControllerTemplate;
        public static NetworkHitboxController NETWORK_HITBOX => _instance._networkHitboxTemplate;
        public static WorldAbilityController ABILITY_CONTROLLER => _instance._worldAbilityTemplate;
        public static VisualFxController VISUAL_FX => _instance._visualFxTemplate;

        private static FactoryController _instance;

        [Header("Unit Templates")]
        [SerializeField] private UnitController _unitControllerTemplate;
        [SerializeField] private TraitController _traitControllerTemplate;
        [SerializeField] private SpriteController _spriteControllerTemplate;
        [SerializeField] private NetworkHitboxController _networkHitboxTemplate;
        [SerializeField] private WorldAbilityController _worldAbilityTemplate;
        [SerializeField] private VisualFxController _visualFxTemplate;

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