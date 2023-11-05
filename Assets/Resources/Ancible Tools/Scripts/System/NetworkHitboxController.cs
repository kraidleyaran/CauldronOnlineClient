using System;
using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class NetworkHitboxController : MonoBehaviour
    {
        private GameObject _owner;

        private HitboxController _controller = null;

        private Action<GameObject> _applyOnCollision = null;

        private bool _reapply = false;

        private List<GameObject> _applyToObjects = new List<GameObject>();

        private Sequence _applySequence = null;

        public void Setup(Hitbox.Hitbox hitbox, CollisionLayer collisionLayer, Action<GameObject> applyOnCollision, bool reApply)
        {
            _controller = Instantiate(hitbox.Controller, transform.parent);
            _controller.Setup(collisionLayer);
            _applyOnCollision = applyOnCollision;
            _controller.AddSubscriber(gameObject);
            _reapply = reApply;
            SubscribeToMessages();
        }

        public void SetSize(Vector2Int size)
        {
            _controller.transform.SetLocalScaling(size);
        }

        public void SetOffset(WorldVector2Int offset)
        {
            _controller.transform.SetLocalPosition(offset.ToWorldVector());
        }

        private void StartApplyTimer()
        {
            if (_applySequence == null)
            {
                _applySequence = DOTween.Sequence().AppendInterval(TickController.WorldTick * CombatManager.Settings.GlobalReApplyTicks).OnComplete(ApplyTimerFinished);
            }
            
        }

        private void ApplyTimerFinished()
        {
            _applySequence = null;
            _applyToObjects.RemoveAll(o => !o);
            if (_applyToObjects.Count > 0 && _applyOnCollision != null)
            {
                foreach (var obj in _applyToObjects)
                {
                    _applyOnCollision?.Invoke(obj);
                }
                StartApplyTimer();
            }
        }

        private void SubscribeToMessages()
        {
            var filter = $"{GetInstanceID()}";
            gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, filter);
            if (_reapply)
            {
                gameObject.SubscribeWithFilter<ExitCollisionWithObjectMessage>(ExitCollisionWithObject, filter);
            }
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            _applyOnCollision?.Invoke(msg.Object);
            if (_reapply && !_applyToObjects.Contains(msg.Object))
            {
                _applyToObjects.Add(msg.Object);
                if (_applySequence == null)
                {
                    StartApplyTimer();
                }
            }
        }

        private void ExitCollisionWithObject(ExitCollisionWithObjectMessage msg)
        {
            if (_reapply && _applyToObjects.Contains(msg.Object))
            {
                _applyToObjects.Remove(msg.Object);
            }
        }

        public void Destroy()
        {
            if (_applySequence != null)
            {
                if (_applySequence.IsActive())
                {
                    _applySequence.Kill();
                }

                _applySequence = null;
            }
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}