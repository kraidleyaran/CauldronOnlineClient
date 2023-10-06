using System;
using System.Collections.Generic;
using ConcurrentMessageBus;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    public class AddTraitToUnitMessage : EventMessage
    {
        public Trait Trait;
        public string WorldId;
        public Action<TraitController> DoAfter;
    }

    public class RemoveTraitFromUnitMessage : EventMessage
    {
        public Trait Trait;
    }

    public class RemoveTraitFromUnitByControllerMessage : EventMessage
    {
        public TraitController Controller;
    }

    public class TraitCheckMessage : EventMessage
    {
        public List<Trait> TraitsToCheck;
        public Action DoAfter;
    }

    public class CacheUnitMessage : EventMessage
    {
        public static CacheUnitMessage INSTANCE = new CacheUnitMessage();
    }


}