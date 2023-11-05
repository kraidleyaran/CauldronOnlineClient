using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    [Serializable]
    public class ColorChange
    {
        public Color Origin = Color.white;
        public string OriginProperty;
        public Color Destination = Color.white;
        public string DestinationProperty;

        public ColorChange()
        {
            Origin = Color.white;
            Destination = Color.white;
        }
    }
}