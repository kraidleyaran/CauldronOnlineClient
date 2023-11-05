using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Skills
{
    public class UiSkillController : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _skillNameText;
        [SerializeField] private UiFillBarController _experienceBarController;
        [SerializeField] private Text _experienceText;
        [SerializeField] private RectTransform _cursorPosition;

        public SkillInstance Skill;
        public Vector2Int Position;

        private bool _hovered = false;

        public void Setup(SkillInstance skill)
        {
            Skill = skill;
            _iconImage.sprite = Skill.Skill.Icon;
            _skillNameText.text = Skill.Skill.DisplayName;
            RefreshSkill();
        }

        public void RefreshSkill()
        {
            _levelText.text = $"{Skill.Level + 1}";
            var required = Skill.Skill.GetRequiredExperience(Skill.Level + 1);
            var experiencePercent = (float)Skill.Experience / required;
            _experienceBarController.Setup(experiencePercent);
            _experienceText.text = $"{Skill.Experience}/{required}";
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.gameObject.SetActive(true);
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void SetHover(bool hover)
        {
            if (_hovered && !hover)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
            else if (!_hovered && hover)
            {
                UiHoverInfoManager.SetHoverInfo(gameObject, $"{Skill.Skill.DisplayName}", $"{Skill.Skill.Description}", Skill.Skill.Icon, transform.position.ToVector2());
            }

            _hovered = hover;
        }
    }
}