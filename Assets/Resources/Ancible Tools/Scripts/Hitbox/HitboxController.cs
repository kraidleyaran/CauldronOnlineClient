using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Hitbox
{
    public class HitboxController : MonoBehaviour
    {
        public Collider Collider { get; private set; }
        public Collider2D Collider2d { get; private set; }

        protected internal List<GameObject> _subscribers = new List<GameObject>();
        protected internal Hitbox _hitbox = null;

        private string _filter = string.Empty;

        public static string GenerateHitboxFilter(Hitbox hitbox, CollisionLayer layer)
        {
            return $"{hitbox.name} {layer.LayerName}";
        }

        public void Setup(Hitbox hitbox, CollisionLayer layer)
        {
            _hitbox = hitbox;
            Collider = gameObject.GetComponent<Collider>();
            Collider2d = gameObject.GetComponent<Collider2D>();
            gameObject.layer = layer.ToLayer();
            _filter = GenerateHitboxFilter(_hitbox, layer);
            SubscribeToMessages();
        }

        public void Setup(CollisionLayer layer)
        {
            Collider = gameObject.GetComponent<Collider>();
            Collider2d = gameObject.GetComponent<Collider2D>();
            gameObject.layer = layer.ToLayer();
        }

        public void AddSubscriber(GameObject subscriber)
        {
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
        }

        public void RemoveSubscriber(GameObject subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public void SetSize(int size)
        {
            transform.SetLocalScaling(new Vector2(size, size));
        }

        public Vector2 GetSize(bool initialized)
        {
            if (initialized)
            {
                return Collider2d ? Collider2d.bounds.size.ToVector2() : Collider.bounds.size.ToVector2();
            }
            else
            {
                var collider2d = gameObject.GetComponent<Collider2D>();
                if (collider2d)
                {
                    return collider2d.bounds.size.ToVector2();
                }
                else
                {
                    var collider = gameObject.GetComponent<Collider>();
                    if (collider)
                    {
                        return collider.bounds.size.ToVector2();
                    }
                }
                
            }

            return Vector2.zero;

        }

        public bool ContainsPoint(Vector2 vector)
        {
            return Collider2d ? Collider2d.bounds.Contains(vector) : Collider.bounds.Contains(vector);
        }


        private void SubscribeToMessages()
        {
            var parent = transform.parent;
            if (parent)
            {
                parent.gameObject.SubscribeWithFilter<HitboxCheckMessage>(HitboxCheck, _filter);
            }
            gameObject.SubscribeWithFilter<RegisterCollisionMessage>(RegisterCollision, _filter);
            gameObject.SubscribeWithFilter<UnregisterCollisionMessage>(UnregisterCollision, _filter);
        }

        private void HitboxCheck(HitboxCheckMessage msg)
        {
            msg.DoAfter.Invoke(this);
        }

        private void RegisterCollision(RegisterCollisionMessage msg)
        {
            _subscribers.Add(msg.Object);
        }

        private void UnregisterCollision(UnregisterCollisionMessage msg)
        {
            _subscribers.Remove(msg.Object);
            if (_subscribers.Count <= 0)
            {
                Destroy();
                Destroy(gameObject);
            }
        }

        public void Destroy()
        {
            transform.parent.gameObject.UnsubscribeFromAllMessagesWithFilter(_filter);
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}