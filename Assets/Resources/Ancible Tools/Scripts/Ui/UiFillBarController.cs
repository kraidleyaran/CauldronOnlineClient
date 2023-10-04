using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiFillBarController : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private float _minimumFill = 0f;
        [SerializeField] private float _maximumFill = 1f;
        [SerializeField] private int _pixelCount = 10;
        [SerializeField] private bool _setSize = false;
        [SerializeField] private RectTransform.Axis _fillDirection = RectTransform.Axis.Horizontal;

        public void Setup(float percent)
        {
            if (_setSize)
            {
                var diff = _maximumFill - _minimumFill;
                var fill = diff * percent;
                _fillImage.rectTransform.SetSizeWithCurrentAnchors(_fillDirection, _minimumFill + fill);
            }
            else
            {
                var fill = 0f;
                if (percent > 0f)
                {
                    var pixelCount = (int)(_pixelCount * percent);
                    fill = (_maximumFill - _minimumFill) / _pixelCount * pixelCount + _minimumFill;
                }
                _fillImage.fillAmount = fill;
            }



        }
    }
}