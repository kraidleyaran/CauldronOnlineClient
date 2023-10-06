using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class TickController : MonoBehaviour
    {
        public const float WORLD_MILLISECONDS = 1000f;

        public static float TickRate => 1f / Application.targetFrameRate;
        public static float WorldTick => _instance._worldTick / WORLD_MILLISECONDS;
        public static WorldTick ServerTick => _instance._serverTick;

        private static TickController _instance = null;

        private WorldTick _serverTick = new WorldTick();

        private int _worldTick = 1;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            SubscribeToMessages();
        }

        public static float CalculateFixedMoveSpeed(int moveSpeed, bool isLocal = false)
        {
            return moveSpeed * DataController.Interpolation * (Time.fixedDeltaTime / (WorldTick + TickRate * 3));
        }

        public static float CalculateUpdateTickMoveSpeed(int moveSpeed)
        {
            return moveSpeed * DataController.Interpolation * (Time.deltaTime / WorldTick);
        }

        void Update()
        {
            gameObject.SendMessage(UpdateTickMessage.INSTANCE);
        }

        void FixedUpdate()
        {
            gameObject.SendMessage(FixedUpdateTickMessage.INSTANCE);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClientWorldSettingsResultMessage>(ClientWorldSettingsResult);
            gameObject.Subscribe<ClientZoneTickMessage>(ClientZoneTick);
        }

        private void ClientWorldSettingsResult(ClientWorldSettingsResultMessage msg)
        {
            _worldTick = msg.WorldTick;
            WorldEventManager.SetRunningState(true);
        }

        private void ClientZoneTick(ClientZoneTickMessage msg)
        {
            _serverTick = msg.Tick;
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}