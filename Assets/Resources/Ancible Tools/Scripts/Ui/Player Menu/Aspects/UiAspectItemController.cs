using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Aspects;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Aspects
{
    public class UiAspectItemController : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _currentPointsText;
        [SerializeField] private Text _additionalPointsText;
        [SerializeField] private RectTransform _cursorPosition;

        [HideInInspector] public int AdditionalPoints;
        [HideInInspector] public WorldAspectInstance Aspect;
        [HideInInspector] public Vector2Int Position;

        private bool _hovered = false;

        public void Setup(WorldAspectInstance instance, Vector2Int gridPosition)
        {
            Aspect = instance;
            Position = gridPosition;
            AdditionalPoints = 0;
            _iconImage.sprite = Aspect.Aspect.Icon;
            //TODO: Color current points text to show bonus if bonus is > 0;
            _currentPointsText.text = $"{Aspect.Rank + Aspect.Bonus} / {Aspect.Aspect.MaxRanks}";
            _additionalPointsText.text = string.Empty;
        }

        public void ApplyPoints(int points)
        {
            AdditionalPoints += points;
            _additionalPointsText.text = $"+{AdditionalPoints}";
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.gameObject.SetActive(true);
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);

        }

        public void SetHovered(bool hovered)
        {
            if (_hovered && !hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
            else if (!_hovered && hovered)
            {
                UiHoverInfoManager.SetHoverInfo(gameObject, Aspect.Aspect.DisplayName, Aspect.Aspect.GetDescription(Aspect.Rank + Aspect.Bonus + 1), Aspect.Aspect.Icon, transform.position.ToVector2());
            }
            _hovered = hovered;
        }

        public void Clear()
        {
            if (_hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
        }
    }
}