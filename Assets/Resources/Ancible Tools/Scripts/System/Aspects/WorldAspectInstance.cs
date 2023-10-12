using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Aspects
{
    public class WorldAspectInstance
    {
        public WorldAspect Aspect;
        public int Rank;
        public int Bonus;

        public WorldAspectInstance(WorldAspect aspect)
        {
            Aspect = aspect;
            Rank = 0;
            Bonus = 0;
        }

        public void Apply(int ranks, GameObject owner, bool isBonus)
        {
            if (isBonus)
            {
                for (var i = 0; i < ranks; i++)
                {
                    Bonus++;
                    Aspect.ApplyRank(Rank + Bonus, owner);
                }
            }
            else
            {
                var applyRanks = Aspect.MaxRanks - Rank;
                if (applyRanks >= 0)
                {
                    for (var i = 0; i < ranks; i++)
                    {
                        Rank++;
                        Aspect.ApplyRank(Rank + Bonus, owner);
                    }
                }
            }
        }

        public AspectData GetData()
        {
            return new AspectData {Name = Aspect.name, Rank = Rank};
        }
    }
}