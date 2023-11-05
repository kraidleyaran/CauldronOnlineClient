using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Skills
{
    public class UiSkillsPlayerMenu : UiPlayerMenu
    {
        [SerializeField] private UiSkillController _skillTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows = 4;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;

        private DataRow<SkillInstance>[] _skills = new DataRow<SkillInstance>[0];
        private Dictionary<Vector2Int, UiSkillController> _controllers = new Dictionary<Vector2Int, UiSkillController>();

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;
        private bool _hover = false;

        void Awake()
        {
            RefreshSkills();
            SubscribeToMessages();
        }

        private void RefreshSkills()
        {
            var querySkillsMsg = MessageFactory.GenerateQuerySkillsMsg();
            querySkillsMsg.DoAfter = UpdateSkills;
            gameObject.SendMessageTo(querySkillsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(querySkillsMsg);
        }

        private void UpdateSkills(SkillInstance[] skills)
        {
            var dataRows = new List<DataRow<SkillInstance>>();
            var dataRow = new DataRow<SkillInstance>();
            dataRows.Add(dataRow);
            foreach (var skill in skills)
            {
                dataRow.Items.Add(skill);
                if (dataRow.Items.Count >= _grid.constraintCount)
                {
                    var available = dataRows.Count * _grid.constraintCount;
                    if (available < skills.Length)
                    {
                        dataRow = new DataRow<SkillInstance>();
                        dataRows.Add(dataRow);
                    }
                }
            }

            _skills = dataRows.ToArray();
            RefreshRows();
        }

        private void RefreshRows()
        {
            _cursor.transform.SetParent(transform);
            _cursor.gameObject.SetActive(false);

            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                Destroy(controller.gameObject);
            }
            _controllers.Clear();

            if (_skills.Length > 0)
            {
                var startingRow = (_skills.Length - _maxRows) < 0 ? 0 : _dataRowPosition;
                var endingRow = _skills.Length - 1;
                if (_skills.Length > _maxRows)
                {
                    endingRow = Mathf.Min(_dataRowPosition + _maxRows - 1, _skills.Length - 1);
                }


                var position = Vector2Int.zero;
                for (var row = startingRow; row <= endingRow; row++)
                {
                    var itemRow = _skills[row];
                    foreach (var item in itemRow.Items)
                    {
                        var controller = Instantiate(_skillTemplate, _grid.transform);
                        controller.Setup(item);
                        controller.Position = position;
                        _controllers.Add(position, controller);
                        position.x++;
                    }

                    position.x = 0;
                    position.y++;
                }

                if (_controllers.TryGetValue(_cursorPosition, out var setCursorController))
                {
                    setCursorController.SetCursor(_cursor);
                    setCursorController.SetHover(_hover);
                }
                else
                {
                    var closest = _controllers.OrderBy(c => (c.Value.Position - _cursorPosition).sqrMagnitude).FirstOrDefault();
                    if (closest.Value)
                    {
                        _cursorPosition = closest.Key;
                        closest.Value.SetCursor(_cursor);
                        closest.Value.SetHover(_hover);;
                    }

                }
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<SkillsUpdatedMessage>(SkillsUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Info && msg.Current.Info)
            {
                _hover = !_hover;
                if (_controllers.TryGetValue(_cursorPosition, out var selected))
                {
                    selected.SetHover(_hover);
                }
            }

            var direction = Vector2Int.zero;
            if (!msg.Previous.Up && msg.Current.Up)
            {
                direction.y = -1;
            }
            else if (!msg.Previous.Down && msg.Current.Down)
            {
                direction.y = 1;
            }
            else if (!msg.Previous.Left && msg.Current.Left)
            {
                direction.x = -1;
            }
            else if (!msg.Previous.Right && msg.Current.Right)
            {
                direction.x = 1;
            }

            if (direction != Vector2Int.zero)
            {

                if (_controllers.TryGetValue(direction + _cursorPosition, out var itemController))
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var selected))
                    {
                        selected.SetHover(false);
                    }
                    _cursorPosition = itemController.Position;
                    itemController.SetCursor(_cursor);
                    itemController.SetHover(_hover);
                }
                else if (direction.y > 0 && _dataRowPosition + 1 < _skills.Length - _maxRows + 1)
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var selected))
                    {
                        selected.SetHover(false);
                    }
                    _dataRowPosition++;
                    RefreshRows();
                }
                else if (direction.y < 0 && _dataRowPosition > 0)
                {
                    if (_controllers.TryGetValue(_cursorPosition, out var selected))
                    {
                        selected.SetHover(false);
                    }
                    _dataRowPosition--;
                    RefreshRows();
                }

                _upArrow.gameObject.SetActive(_dataRowPosition > 0);
                _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _skills.Length - _maxRows + 1);
            }
        }

        private void SkillsUpdated(SkillsUpdatedMessage msg)
        {
            RefreshSkills();
        }
        
    }
}