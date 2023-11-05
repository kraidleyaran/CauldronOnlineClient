using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions
{
    [CreateAssetMenu(fileName = "Hair Color Option", menuName = "Ancible Tools/Color Options/Hair Color Options")]
    public class HairColorOption : ColorOption
    {
        public const string PRIMARY_COLOR_PROPERTY = "_HairColorOne";
        public const string SECONDARY_COLOR_PROPERTY = "_HairColorTwo";

        [SerializeField] private Color _primaryColor = Color.white;
        [SerializeField] private Color _secondaryColor = Color.white;

        public override void Apply(Material material, bool secondary)
        {
            material.SetColor(PRIMARY_COLOR_PROPERTY, _primaryColor);
            material.SetColor(SECONDARY_COLOR_PROPERTY, _secondaryColor);
        }
    }
}