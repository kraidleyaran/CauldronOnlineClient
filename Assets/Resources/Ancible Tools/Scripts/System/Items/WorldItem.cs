using Assets.Resources.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "World Item", menuName = "Ancible Tools/Items/General Item")]
    public class WorldItem : ScriptableObject
    {
        public virtual ItemType Type => ItemType.General;
        public string DisplayName;
        public SpriteTrait Sprite;
        [TextArea(3, 5)] public string Description;
        public int MaxStack = 1;
        public int SellValue = 0;

        public virtual string GetDescription()
        {
            return Description;
        }
    }
}