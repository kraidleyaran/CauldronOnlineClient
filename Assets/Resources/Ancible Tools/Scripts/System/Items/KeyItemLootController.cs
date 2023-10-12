using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class KeyItemLootController : MonoBehaviour
    {
        [SerializeField] private int _moveDistance = 1;
        [SerializeField] private int _moveTime = 1;
        [SerializeField] private int _idleTime = 1;

        private SpriteController _itemSprite = null;

        private Tween _moveTween = null;

        public void Setup(WorldItem item)
        {
            _itemSprite = Instantiate(FactoryController.SPRITE_CONTROLLER, transform);
            _itemSprite.SetSprite(item.Sprite.Sprite);
            _itemSprite.SetScale(item.Sprite.Scaling);
            _itemSprite.SetOffset(item.Sprite.Offset);
            _itemSprite.SetSortingLayerFromSpriteLayer(item.Sprite.SpriteLayer);
            var movePosition = transform.position.ToVector2() + Vector2.up * (_moveDistance * DataController.Interpolation);
            _moveTween = transform.DOMove(movePosition, _moveTime * TickController.TickRate).OnComplete(MoveFinished);
        }

        private void MoveFinished()
        {
            _moveTween = DOTween.Sequence().AppendInterval(_idleTime * TickController.TickRate).OnComplete(IdleFinished);
        }

        private void IdleFinished()
        {
            _moveTween = null;
            ObjectManager.DestroyNetworkObject(gameObject);
        }

        void OnDestroy()
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }
        }
    }
}