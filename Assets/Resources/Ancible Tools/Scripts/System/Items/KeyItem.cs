using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Key Item", menuName = "Ancible Tools/Items/Key Item")]
    public class KeyItem : WorldItem
    {
        public override ItemType Type => ItemType.Key;
    }
}