using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Terrain Trait", menuName = "Ancible Tools/Traits/General/Terrain")]
    public class TerrainTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox;

        private HitboxController _hitboxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.Terrain);
            _hitboxController.AddSubscriber(_controller.gameObject);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupTerrainMessage>(SetupTerrain, _instanceId);
        }

        private void SetupTerrain(SetupTerrainMessage msg)
        {
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
            if (msg.IsGround)
            {
                _hitboxController.gameObject.layer = CollisionLayerFactory.GroundTerrain.ToLayer();
            }
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
                unregisterCollisionMsg.Object = _controller.gameObject;
                _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _hitboxController.gameObject);
                MessageFactory.CacheMessage(unregisterCollisionMsg);
            }

            _hitboxController = null;
            base.Destroy();
        }
    }
}