using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [CreateAssetMenu(fileName = "Bonus Tag", menuName = "Ancible Tools/Items/Bonus Tag")]
    public class BonusTag : ScriptableObject
    {
        public string DisplayName;
        public bool Show = true;
    }
}