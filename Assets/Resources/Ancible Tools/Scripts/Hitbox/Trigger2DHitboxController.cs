using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Hitbox
{
    public class Trigger2DHitboxController : HitboxController
    {
        void OnTriggerEnter2D(Collider2D col)
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

        void OnTriggerExit2D(Collider2D col)
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