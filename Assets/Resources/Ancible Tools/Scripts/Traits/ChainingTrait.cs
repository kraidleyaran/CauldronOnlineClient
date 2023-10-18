using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Chaining Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Chaining")]
    public class ChainingTrait : Trait
    {
        [SerializeField] private int _maxChains = 1;
        [SerializeField] private int _chainArea = 1;
        [SerializeField] private Hitbox.Hitbox _chainAreaHitbox;
        [SerializeField] private Hitbox.Hitbox _chainChangeHitbox;
        [SerializeField] private CollisionLayer _changeLayer = null;
        [SerializeField] private CollisionLayer _areaLayer = null;
        [SerializeField] private int _maxObjectHistory = 1;
        [SerializeField] private Trait[] _applyOnChainEnd = new Trait[0];
        [SerializeField] private bool _resetMaxDistanceChain = false;

        private HitboxController _chainAreaController;
        private HitboxController _chainChangeController;
        private ContactFilter2D _filter = new ContactFilter2D();

        private List<GameObject> _objectHistory = new List<GameObject>();

        private int _chainsRemaining = 0;

        private GameObject _currentTarget = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _chainsRemaining = _maxChains;
            _filter = new ContactFilter2D {useTriggers = true, layerMask = _areaLayer.ToMask(), useLayerMask = true};
            _chainAreaController = Instantiate(_chainAreaHitbox.Controller, _controller.transform.parent);
            _chainAreaController.Setup(_chainAreaHitbox, CollisionLayerFactory.Casting);
            _chainAreaController.SetSize(_chainArea);
            _chainChangeController = _controller.gameObject.SetupHitbox(_chainChangeHitbox, _changeLayer);
            _chainChangeController.AddSubscriber(_controller.gameObject);
            SubscribeToMessages();
        }

        private void UpdateTargetDirection()
        {
            var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
            setDirectionMsg.Direction = (_currentTarget.transform.position.ToVector2() - _controller.transform.position.ToVector2()).ToDirection();
            _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setDirectionMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReturningToOwnerMessage>(ReturningToOwner, _instanceId);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            Debug.Log($"Chain Connected");
            _objectHistory.Add(msg.Object);
            while (_objectHistory.Count > _maxObjectHistory + 1)
            {
                _objectHistory.RemoveAt(0);
            }
            var chained = false;
            if (_chainsRemaining > 0)
            {
                var results = new RaycastHit2D[_chainsRemaining];
                var resultCount = _chainAreaController.Collider2d.Cast(Vector2.down, _filter, results, .3125f, true);
                
                Debug.Log($"Chain Results: {resultCount}");
                if (resultCount > 0)
                {
                    var orderedResults = results.ToList().GetRange(0, resultCount).OrderBy(o => o.distance).ToArray();
                    for (var i = 0; i < resultCount; i++)
                    {
                        var result = orderedResults[i];
                        if (!_objectHistory.Contains(result.transform.gameObject))
                        {
                            if (_currentTarget)
                            {
                                _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
                            }
                            _currentTarget = result.transform.gameObject;
                            _currentTarget.SubscribeWithFilter<UpdateWorldPositionMessage>(TargetUpdateWorldPosition, _instanceId);
                            _objectHistory.Add(_currentTarget);
                            break;
                        }
                    }
                }
            }
            _chainsRemaining--;
            if (_resetMaxDistanceChain)
            {
                _controller.gameObject.SendMessageTo(ResetMaxDistanceCheckMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
            if (_chainsRemaining < 0)
            {
                if (_applyOnChainEnd.Length > 0)
                {
                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in _applyOnChainEnd)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);
                }

                var removeTraitFromUnitMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removeTraitFromUnitMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraitFromUnitMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraitFromUnitMsg);
            }
        }

        private void TargetUpdateWorldPosition(UpdateWorldPositionMessage msg)
        {
            UpdateTargetDirection();
        }

        private void ReturningToOwner(ReturningToOwnerMessage msg)
        {
            var removeTraitFromUnitMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitFromUnitMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitFromUnitMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitFromUnitMsg);
        }

        public override void Destroy()
        {
            Destroy(_chainAreaController.gameObject);

            var unregisterCollisionMsg = MessageFactory.GenerateUnregisterCollisionMsg();
            unregisterCollisionMsg.Object = _controller.gameObject;
            _controller.gameObject.SendMessageTo(unregisterCollisionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(unregisterCollisionMsg);

            if (_currentTarget)
            {
                _currentTarget.UnsubscribeFromAllMessagesWithFilter(_instanceId);
            }

            base.Destroy();
        }
    }
}