using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Bridge Trait", menuName = "Ancible Tools/Traits/Interactable/Bridge")]
    public class BridgeTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox = null;

        private STETilemap _tilemapSprite = null;
        private HitboxController _hitboxController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = Instantiate(_hitbox.Controller, _controller.transform.parent);
            _hitboxController.Setup(CollisionLayerFactory.Terrain);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupBridgeMessage>(SetupBridge, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetBridgeStateMessage>(SetBridgeState, _instanceId);
        }

        private void SetupBridge(SetupBridgeMessage msg)
        {
            var bridgeSprite = TraitFactory.GetTilemapSprite(msg.TilemapSprite);
            if (bridgeSprite)
            {
                _tilemapSprite = Instantiate(bridgeSprite.Tilemap, _controller.transform.parent);
                _tilemapSprite.gameObject.SetActive(msg.Active);
            }

            _hitboxController.transform.SetLocalScaling(msg.Size.ToVector());
            _hitboxController.gameObject.SetActive(!msg.Active);
        }

        private void SetBridgeState(SetBridgeStateMessage msg)
        {
            _tilemapSprite?.gameObject.SetActive(msg.Active);
            _hitboxController.gameObject.SetActive(!msg.Active);
        }

        public override void Destroy()
        {
            if (_tilemapSprite)
            {
                Destroy(_tilemapSprite.gameObject);
            }
            _hitboxController.Destroy();
            Destroy(_hitboxController.gameObject);
            base.Destroy();
        }
    }
}