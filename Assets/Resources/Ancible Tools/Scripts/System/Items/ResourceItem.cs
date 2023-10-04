using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Resource Item", menuName = "Ancible Tools/Items/Resource Item")]
    public class ResourceItem : WorldItem
    {
        public override ItemType Type => ItemType.Resource;
    }
}