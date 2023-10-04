using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server
{
    [Serializable]
    public class ServerHitbox
    {
        public Vector2Int Size = Vector2Int.one;
        public Vector2Int Offset = Vector2Int.zero;


        public HitboxData GetData()
        {
            return new HitboxData
            {
                Size = Size.ToWorldVector(),
                Offset = Offset.ToWorldVector(),
            };
        }
    }

    [Serializable]
    public class ApplyServerHitbox : ServerHitbox
    {
        public ServerTrait[] ApplyOnServer = new ServerTrait[0];
        public Trait[] ApplyOnClient = new Trait[0];

        public ApplyHitboxData GetApplyData()
        {
            return new ApplyHitboxData
            {
                Size = Size.ToWorldVector(),
                Offset = Offset.ToWorldVector(),
                ApplyOnServer = ApplyOnServer.Where(t => t).Select(t => t.name).ToArray(),
                ApplyOnClient = ApplyOnClient.Where(t => t).Select(t => t.name).ToArray()
            };
        }
    }
}