using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class CollisionLayerFactory : MonoBehaviour
    {
        public static CollisionLayer MonsterAggro => _instance._monsterAggro;
        public static CollisionLayer PlayerAggro => _instance._playerAggro;
        public static CollisionLayer Casting => _instance._casting;
        public static CollisionLayer Terrain => _instance._terrain;
        public static CollisionLayer Interaction => _instance._interaction;
        public static CollisionLayer ItemCollect => _instance._itemCollect;
        public static CollisionLayer ZoneTransfer => _instance._zoneTransfer;

        private static CollisionLayerFactory _instance = null;

        [SerializeField] private CollisionLayer _monsterAggro;
        [SerializeField] private CollisionLayer _playerAggro;
        [SerializeField] private CollisionLayer _casting;
        [SerializeField] private CollisionLayer _terrain;
        [SerializeField] private CollisionLayer _interaction;
        [SerializeField] private CollisionLayer _itemCollect;
        [SerializeField] private CollisionLayer _zoneTransfer;

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