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
        [SerializeField] private Vector2 _evenOffsets = new Vector2(2.5f, 2.5f);

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
                _tilemapSprite.transform.SetLocalPosition(bridgeSprite.Offset.ToVector2(true));
                _tilemapSprite.gameObject.SetActive(msg.Active);
            }

            _hitboxController.transform.SetLocalScaling(msg.Size.ToVector());
            var localPos = _hitboxController.transform.localPosition.ToVector2();
            if (msg.Size.X % 2 == 0)
            {
                localPos.x += _evenOffsets.x;
            }
            if (msg.Size.Y % 2 == 0)
            {
                localPos.y += _evenOffsets.y;
            }
            _hitboxController.transform.SetLocalPosition(localPos);
            _hitboxController.gameObject.SetActive(!msg.Active);
        }

        private void SetBridgeState(SetBridgeStateMessage msg)
        {
            _tilemapSprite?.gameObject.SetActive(msg.Active);
            _hitboxController.gameObject.SetActive(!msg.Active);

            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            addTraitToUnitMsg.Trait = TraitFactory.AppearanceFx;
            _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(addTraitToUnitMsg);
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