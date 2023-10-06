using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Hitbox
{
    public class TriggerHitboxController : HitboxController
    {
        void OnTriggerEnter(Collider col)
        {
            if (col.transform.parent)
            {
                var enterCollisionMsg = MessageFactory.GenerateEnterCollisionWithObjectMsg();
                enterCollisionMsg.Object = col.transform.parent.gameObject;
                for (var i = 0; i < _subscribers.Count; i++)
                {
                    gameObject.SendMessageTo(enterCollisionMsg, _subscribers[i]);
                }
                MessageFactory.CacheMessage(enterCollisionMsg);
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (col.transform.parent)
            {
                var exitCollisionMsg = MessageFactory.GenerateExitCollisionWithObjectMsg();
                exitCollisionMsg.Object = col.transform.parent.gameObject;
                for (var i = 0; i < _subscribers.Count; i++)
                {
                    gameObject.SendMessageTo(exitCollisionMsg, _subscribers[i]);
                }
                MessageFactory.CacheMessage(exitCollisionMsg);
            }
        }
    }
}