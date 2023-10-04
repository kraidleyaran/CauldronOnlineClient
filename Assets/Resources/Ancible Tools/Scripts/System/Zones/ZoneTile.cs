using System;
using RogueSharp;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Zones
{
    [Serializable]
    public class ZoneTile
    {
        public Vector2Int Tile;
        public Vector2 WorldCenter;
        public ICell Cell;
    }
}