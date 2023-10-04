using System;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using CauldronOnlineCommon.Data.Math;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class NetworkHitboxController : MonoBehaviour
    {
        private GameObject _owner;

        private HitboxController _controller = null;

        private Action<GameObject> _applyOnCollision = null;

        public void Setup(Hitbox.Hitbox hitbox, CollisionLayer collisionLayer, Action<GameObject> applyOnCollision)
        {
            _controller = Instantiate(hitbox.Controller, transform.parent);
            _controller.Setup(collisionLayer);
            _applyOnCollision = applyOnCollision;
            _controller.AddSubscriber(gameObject);
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

        private void SubscribeToMessages()
        {
            var filter = $"{GetInstanceID()}";
            gameObject.SubscribeWithFilter<EnterCollisionWithObjectMessage>(EnterCollisionWithObject, filter);
        }

        private void EnterCollisionWithObject(EnterCollisionWithObjectMessage msg)
        {
            _applyOnCollision?.Invoke(msg.Object);
        }

        public void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}