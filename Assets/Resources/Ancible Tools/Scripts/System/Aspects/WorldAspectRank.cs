using System;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Aspects
{
    [Serializable]
    public class WorldAspectRank
    {
        public int MinimumLevel;
        public Trait[] ApplyOnLevel;

        public void Apply(GameObject owner)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            foreach (var trait in ApplyOnLevel)
            {
                addTraitToUnitMsg.Trait = trait;
                owner.SendMessageTo(addTraitToUnitMsg, owner);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }
    }
}