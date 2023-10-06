using System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Hitbox
{
    public class EnterCollisionWithObjectMessage : EventMessage
    {
        public GameObject Object;
    }

    public class ExitCollisionWithObjectMessage : EventMessage
    {
        public GameObject Object;
    }

    public class HitboxCheckMessage : EventMessage
    {
        public Action<HitboxController> DoAfter;
    }

    public class RegisterCollisionMessage : EventMessage
    {
        public GameObject Object;
    }

    public class UnregisterCollisionMessage : EventMessage
    {
        public GameObject Object;
    }
}