using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions
{
    [CreateAssetMenu(fileName = "Eyes Color Option", menuName = "Ancible Tools/Color Options/Eyes Color Options")]
    public class EyesColorOption : ColorOption
    {
        public const string PRIMARY_COLOR_PROPERTY = "_EyeColor";

        public override ColorOptionType Type => ColorOptionType.Eyes;

        [SerializeField] private Color _eyeColor = Color.white;

        public override void Apply(Material material, bool secondary)
        {
            material.SetColor(PRIMARY_COLOR_PROPERTY, _eyeColor);
        }
    }
}