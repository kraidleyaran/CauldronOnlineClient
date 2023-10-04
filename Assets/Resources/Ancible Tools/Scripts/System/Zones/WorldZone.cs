using System;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Zones
{
    [CreateAssetMenu(fileName = "World Zone", menuName = "Ancible Tools/World Zone")]
    public class WorldZone : ScriptableObject
    {
        public string DisplayName;
        [SerializeField] private string[] _aliases = new string[0];
        [HideInInspector] public Vector2Int DefaultSpawn = Vector2Int.zero;
        public WorldZoneController Controller;

        public WorldZoneData GetData()
        {
            return Controller.GetData(name, _aliases, DefaultSpawn.ToWorldVector());
        }
    }
}