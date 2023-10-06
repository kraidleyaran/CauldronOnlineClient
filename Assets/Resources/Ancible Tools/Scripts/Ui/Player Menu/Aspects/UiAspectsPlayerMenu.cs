using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Aspects;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu.Aspects
{
    public class UiAspectsPlayerMenu : UiPlayerMenu
    {
        [SerializeField] private Text _availablePointsText = null;
        [SerializeField] private UiAspectItemController _aspectTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows = 0;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;

        private DataRow<WorldAspectInstance>[] _rows = new DataRow<WorldAspectInstance>[0];
        private Dictionary<Vector2Int, UiAspectItemController> _controllers = new Dictionary<Vector2Int, UiAspectItemController>();

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;

        private int _availablePoints = 0;

        void Awake()
        {
            RefreshAspects();
            SubscribeToMessages();
        }

        private void RefreshAspects()
        {
            var queryAspectsMsg = MessageFactory.GenerateQueryAspectsMsg();
            queryAspectsMsg.DoAfter = UpdateAspects;
            gameObject.SendMessageTo(queryAspectsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryAspectsMsg);
        }

        private void RefreshRows()
        {
            _cursor.transform.SetParent(transform);
            _cursor.gameObject.SetActive(false);

            if (_controllers.TryGetValue(_cursorPosition, out var hovered))
            {
                hovered.SetHovered(false);
            }

            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                Destroy(controller.gameObject);
            }
            _controllers.Clear();

            var startingRow = (_rows.Length - _maxRows) < 0 ? 0 : _dataRowPosition;
            var endingRow = _rows.Length - 1;
            if (_rows.Length > _maxRows)
            {
                endingRow = Mathf.Min(_dataRowPosition + _maxRows - 1, _rows.Length - 1);
            }

            if (_rows.Length > 0)
            {
                var position = Vector2Int.zero;
                for (var row = startingRow; row <= endingRow; row++)
                {
                    var itemRow = _rows[row];
                    foreach (var item in itemRow.Items)
                    {
                        var controller = Instantiate(_aspectTemplate, _grid.transform);
                        controller.Setup(item, position);
                        _controllers.Add(position, controller);
                        position.x++;
                    }

                    position.x = 0;
                    position.y++;
                }

                if (_controllers.TryGetValue(_cursorPosition, out var setCursorController))
                {
                    setCursorController.SetCursor(_cursor);
                }
                else
                {
                    var closest = _controllers.OrderBy(c => (c.Value.Position - _cursorPosition).sqrMagnitude).FirstOrDefault();
                    if (closest.Value)
                    {
                        _cursorPosition = closest.Key;
                        closest.Value.SetCursor(_cursor);
                    }
                }
            }
        }

        private void UpdateAspects(WorldAspectInstance[] aspects, int availablePoints)
        {
            _availablePoints = availablePoints;
            _availablePointsText.text = $"{_availablePoints}";

            var rows = new List<DataRow<WorldAspectInstance>>();
            var dataRow = new DataRow<WorldAspectInstance>();
            rows.Add(dataRow);

            var ordered = aspects.OrderBy(a => string.IsNullOrEmpty(a.Aspect.DisplayName) ? a.Aspect.name : a.Aspect.DisplayName).ToArray();
            foreach (var aspect in ordered)
            {
                dataRow.Items.Add(aspect);
                if (dataRow.Items.Count >= _grid.constraintCount)
                {
                    var available = rows.Count * _grid.constraintCount;
                    if (available < ordered.Length)
                    {
                        dataRow = new DataRow<WorldAspectInstance>();
                        rows.Add(dataRow);
                    }
                }

            }

            _rows = rows.ToArray();
            RefreshRows();
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.Subscribe<PlayerAspectsUpdatedMessage>(PlayerAspectsUpdated);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var buttonPushed = false;
            if (!msg.Previous.Green && msg.Current.Green && _availablePoints > 0)
            {
                if (_controllers.TryGetValue(_cursorPosition, out var controller))
                {
                    if (controller.Aspect.Rank + controller.AdditionalPoints < controller.Aspect.Aspect.MaxRanks)
                    {
                        _availablePoints--;
                        _availablePointsText.text = $"{_availablePoints}";
                        controller.ApplyPoints(1);
                        buttonPushed = true;
                    }
                }
            }
            else if (!msg.Previous.Red && msg.Current.Red)
            {
                if (_controllers.TryGetValue(_cursorPosition, out var controller))
                {
                    if (controller.AdditionalPoints > 0)
                    {
                        _availablePoints++;
                        _availablePointsText.text = $"{_availablePoints}";
                        controller.ApplyPoints(-1);
                    }
                }
            }
            else if (!msg.Previous.Blue && msg.Current.Blue)
            {
                var controllers = _controllers.Values.Where(c => c.AdditionalPoints > 0).ToArray();
                if (controllers.Length > 0)
                {
                    var applyAspectRanksMsg = MessageFactory.GenerateApplyAspectRanksMsg();
                    applyAspectRanksMsg.Update = false;
                    applyAspectRanksMsg.Bonus = false;
                    foreach (var controller in controllers)
                    {
                        applyAspectRanksMsg.Aspect = controller.Aspect.Aspect;
                        applyAspectRanksMsg.Ranks = controller.AdditionalPoints;
                        gameObject.SendMessageTo(applyAspectRanksMsg, ObjectManager.Player);
                    }
                    MessageFactory.CacheMessage(applyAspectRanksMsg);

                    buttonPushed = true;
                    gameObject.SendMessage(PlayerAspectsUpdatedMessage.INSTANCE);
                }
            }
            else
            {
                if (_controllers.TryGetValue(_cursorPosition, out var controller))
                {
                    controller.SetHovered(msg.Current.Info);
                }
            }

            if (!buttonPushed)
            {
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
                        _cursorPosition = itemController.Position;
                        itemController.SetCursor(_cursor);
                    }
                    else if (direction.y > 0 && _dataRowPosition + 1 < _rows.Length - _maxRows + 1)
                    {
                        _dataRowPosition++;
                        RefreshRows();
                    }
                    else if (direction.y < 0 && _dataRowPosition > 0)
                    {
                        _dataRowPosition--;
                        RefreshRows();
                    }

                    _upArrow.gameObject.SetActive(_dataRowPosition > 0);
                    _downArrow.gameObject.SetActive(_dataRowPosition + 1 < _rows.Length - _maxRows + 1);
                }
            }
            
        }

        private void PlayerAspectsUpdated(PlayerAspectsUpdatedMessage msg)
        {
            RefreshAspects();
        }
    }
}