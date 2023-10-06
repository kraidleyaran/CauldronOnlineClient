using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class ZoneTransferController : MonoBehaviour
    {
        public WorldZone Zone => _zone;

        [SerializeField] private Hitbox.Hitbox _hitbox;
        [SerializeField] private WorldZone _zone;
        [HideInInspector] public Vector2Int Position = Vector2Int.zero;

        private HitboxController _hitboxController = null;

        void Awake()
        {
            _hitboxController = Instantiate(_hitbox.Controller, transform);
            _hitboxController.Setup(CollisionLayerFactory.ZoneTransfer);
            _hitboxController.AddSubscriber(gameObject);
            SubscribeToMessages();
            ObjectManager.RegisterObject(gameObject);
        }

        private void SubscribeToMessages()
        {
            var filter = $"{GetInstanceID()}";
            gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, filter);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (msg.Object == ObjectManager.Player)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Interaction;
                gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(setUnitStateMsg);

                ClientController.TransferPlayer(_zone, Position.ToWorldVector());
            }
        }

        void OnDestroy()
        {
            Destroy(_hitboxController.gameObject);
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}