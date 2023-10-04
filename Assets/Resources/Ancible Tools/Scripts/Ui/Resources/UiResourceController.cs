using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Resources
{
    public class UiResourceController : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Text _stackText;
        [SerializeField] private int _maxDisplayStack = 999;

        public ResourceItem Item;

        public void Setup(ResourceItem item, int stack)
        {
            Item = item;
            _stackText.text = stack > _maxDisplayStack ? $"{_maxDisplayStack}+" : $"{stack}";
        }
    }
}