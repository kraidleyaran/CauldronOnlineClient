using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions
{
    [CreateAssetMenu(fileName = "Shirt Color Option", menuName = "Ancible Tools/Color Options/Shirt Color Options")]
    public class ShirtColorOption : ColorOption
    {
        public const string PRIMARY_SHIRT_PROPERTY = "_PrimaryShirt";
        public const string SECONDARY_SHIRT_PROPERTY = "_SecondaryShirt";
        public const string COLOR1_PROPERTY = "ColorOne";
        public const string COLOR2_PROPERTY = "ColorTwo";
        public const string COLOR3_PROPERTY = "ColorThree";

        public override ColorOptionType Type => ColorOptionType.Shirt;

        [SerializeField] private Color _color1 = Color.white;
        [SerializeField] private Color _color2 = Color.white;
        [SerializeField] private Color _color3 = Color.white;

        public override void Apply(Material material, bool secondary)
        {
            var property = PRIMARY_SHIRT_PROPERTY;
            if (secondary)
            {
                property = SECONDARY_SHIRT_PROPERTY;
            }
            material.SetColor($"{property}{COLOR1_PROPERTY}", _color1);
            material.SetColor($"{property}{COLOR2_PROPERTY}", _color2);
            material.SetColor($"{property}{COLOR3_PROPERTY}", _color3);

        }
    }
}