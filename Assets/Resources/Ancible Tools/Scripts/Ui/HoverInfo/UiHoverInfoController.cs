using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo
{
    public class UiHoverInfoController : MonoBehaviour
    {
        public RectTransform RectTransform;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _titleText;
        [SerializeField] private RectTransform _titleGroup;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _goldText;
        [SerializeField] private RectTransform _goldGroup;
        [SerializeField] private float _baseHeight;
        

        public void Setup(Sprite icon, string title, string description, int amount = 0)
        {
            _iconImage.sprite = icon;
            _iconImage.gameObject.SetActive(icon);
            _titleText.text = $"{title}";
            _descriptionText.text = $"{description}";
            var size = _baseHeight + _titleGroup.rect.height;
            if (!string.IsNullOrEmpty(description))
            {
                var height = _descriptionText.GetHeightOfText(description.RemoveTags());
                _descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                size += height;
            }

            if (amount > 0)
            {
                _goldText.text = $"{amount:n0}";
                size += _goldGroup.rect.height;
            }
            _goldGroup.gameObject.SetActive(amount > 0);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }

        public void SetPivot(Vector2 pivot)
        {
            RectTransform.pivot = pivot;
        }

        public void SetPosition(Vector2 pos)
        {
            transform.SetTransformPosition(pos);
        }

        public void Clear()
        {
            _descriptionText.text = string.Empty;
            _titleText.text = string.Empty;
            _iconImage.sprite = null;
            _iconImage.gameObject.SetActive(false);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _baseHeight);
        }
    }
}