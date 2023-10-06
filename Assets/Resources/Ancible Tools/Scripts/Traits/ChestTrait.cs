using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Chest Trait", menuName = "Ancible Tools/Traits/Interactable/Chest")]
    public class ChestTrait : InteractableTrait
    {
        protected internal override string _actionText => "Open";

        protected internal SpriteTrait _closedSprite;
        protected internal SpriteTrait _openSprite;

        private bool _open = false;

        private SpriteController _sprite = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _sprite = Instantiate(FactoryController.SPRITE_CONTROLLER, _controller.transform.parent);
        }

        protected internal override void Interact()
        {
            if (!_open)
            {
                var objId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(objId))
                {
                    ClientController.SendToServer(new ClientOpenChestMessage {TargetId = objId, Tick = TickController.ServerTick});
                }
            }
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<OpenChestMessage>(OpenChest, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupChestMessage>(SetupChest, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWorldPositionMessage>(UpdateWorldPosition, _instanceId);
        }

        private void OpenChest(OpenChestMessage msg)
        {
            if (!_open)
            {
                _open = true;
                if (_openSprite)
                {
                    _sprite.SetSprite(_openSprite.Sprite);
                    _sprite.SetScale(_openSprite.Scaling);
                    _sprite.SetOffset(_openSprite.Offset);
                }
            }
        }

        private void SetupChest(SetupChestMessage msg)
        {
            _openSprite = TraitFactory.GetSprite(msg.OpenSprite);
            _closedSprite = TraitFactory.GetSprite(msg.CloseSprite);
            _open = msg.Open;
            var sprite = _open ? _openSprite : _closedSprite;
            if (sprite)
            {
                _sprite.SetSprite(sprite.Sprite);
                _sprite.SetScale(sprite.Scaling);
                _sprite.SetOffset(sprite.Offset);
            }
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }

        private void UpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            var additionalOrder = _open ? _openSprite.SortingOrder : _closedSprite.SortingOrder;
            _sprite.SetSortingOrder(msg.Position.Y * -1 + additionalOrder);
        }
    }
}