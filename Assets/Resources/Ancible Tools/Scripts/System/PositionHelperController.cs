using CauldronOnlineCommon.Data.Math;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class PositionHelperController : MonoBehaviour
    {
        public Vector2Int WorldPosition;
        public WorldVector2Int WorldVectorPosition;
        public bool IsPlayerSpawn = false;
        [HideInInspector] public string DisplayName;

        public void RefreshPositions()
        {
#if UNITY_EDITOR
            WorldPosition = (transform.position.ToVector2() / .3125f).ToVector2Int();
            WorldVectorPosition = WorldPosition.ToWorldVector();
#endif
        }
    }
}