using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions
{
    public class ColorOption : ScriptableObject
    {
        public virtual ColorOptionType Type => ColorOptionType.Hair;

        public virtual void Apply(Material material, bool secondary)
        {

        }
    }
}