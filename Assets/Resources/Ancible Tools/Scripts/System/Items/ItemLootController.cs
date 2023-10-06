using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon.Data.Math;
using DG.Tweening;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class ItemLootController : MonoBehaviour
    {
        [SerializeField] private Hitbox.Hitbox _pickupHitbox;
        [SerializeField] private int _jumpSpeed;
        [SerializeField] private int _jumpPower;
        [SerializeField] private WorldIntRange _range;
        [SerializeField] private int _idleTimerTicks = 1;
        [SerializeField] private int _moveToPlayerVelocity;
        [SerializeField] private int _addToPlayerDistance;
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private Rigidbody2D _rigidBody = null;

        private HitboxController _hitboxController = null;
        private SpriteController _spriteController = null;
        private string _filter = string.Empty;

        private WorldItem _item;
        private int _stack = 1;

        private Tween _moveTween = null;
        private Vector2 _direction = Vector2.zero;
        private bool _moveTowardsPlayer = false;
        private int _moveTowardsPlayerSpeed = 0;
        

        public void Setup(WorldItem item, int stack, Vector2 direction)
        {
            _item = item;
            _stack = stack;
            
            if (_item.Sprite)
            {
                _spriteController = Instantiate(FactoryController.SPRITE_CONTROLLER, transform);
                _spriteController.SetScale(item.Sprite.Scaling);
                _spriteController.SetOffset(item.Sprite.Offset);

                if (_item.Sprite.RuntimeController)
                {
                    _spriteController.SetRuntimeController(_item.Sprite.RuntimeController);
                    _spriteController.Play();
                }
                else
                {
                    _spriteController.SetSprite(_item.Sprite.Sprite);
                }
            }
            var currentPos = transform.position.ToVector2();
            var distance = _range.Roll(true) * DataController.Interpolation;
            currentPos += new Vector2(direction.x * distance, direction.y * distance);
            var jumpTime = TickController.TickRate * (distance / (_jumpSpeed * DataController.Interpolation));
            _moveTween = transform.DOJump(currentPos, _jumpPower * DataController.Interpolation, 1, jumpTime).SetEase(_ease).OnComplete(JumpFinished);
            _hitboxController = Instantiate(_pickupHitbox.Controller, transform);
            _hitboxController.Setup(CollisionLayerFactory.ItemCollect);
            _hitboxController.AddSubscriber(gameObject);
            SubscribeToMessages();
        }

        private void JumpFinished()
        {
            _moveTween = DOTween.Sequence().AppendInterval(_idleTimerTicks * TickController.TickRate).OnComplete(IdleFinished);
        }

        private void IdleFinished()
        {
            _moveTween = null;
            if (_moveTowardsPlayer)
            {
                gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
                gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
            }
        }

        private void SubscribeToMessages()
        {
            _filter = $"{GetInstanceID()}";
            gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _filter);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            if (msg.Object == ObjectManager.Player && !_moveTowardsPlayer)
            {
                _moveTowardsPlayer = true;
                if (_moveTween == null)
                {
                    gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
                    gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
                }
                _hitboxController.Destroy();
                Destroy(_hitboxController.gameObject);
                _hitboxController = null;
                gameObject.UnsubscribeFromAllMessagesWithFilter(_filter);
            }
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_moveTowardsPlayer)
            {
                var speed = TickController.CalculateFixedMoveSpeed(_moveTowardsPlayerSpeed, true);
                _rigidBody.position += _direction * speed;
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_moveTowardsPlayer)
            {
                var diff = (ObjectManager.Player.transform.position.ToVector2() - _rigidBody.position);
                var distance = diff.magnitude;
                var checkDistance = _addToPlayerDistance * DataController.Interpolation;
                if (distance <= checkDistance)
                {
                    _moveTowardsPlayer = false;
                    var addItemMsg = MessageFactory.GenerateAddItemMsg();
                    addItemMsg.Item = _item;
                    addItemMsg.Stack = _stack;
                    gameObject.SendMessageTo(addItemMsg, ObjectManager.Player);
                    MessageFactory.CacheMessage(addItemMsg);

                    ObjectManager.DestroyNetworkObject(gameObject);
                }
                else
                {
                    _direction = diff.normalized;
                    _moveTowardsPlayerSpeed += _moveToPlayerVelocity;
                }
            }
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}