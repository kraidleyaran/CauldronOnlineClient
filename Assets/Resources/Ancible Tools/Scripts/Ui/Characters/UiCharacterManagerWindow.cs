using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Characters
{
    public class UiCharacterManagerWindow : UiWindowBase
    {
        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private UiPlayerCharacterController _controllerTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxRows = 4;
        [SerializeField] private GameObject _cursor;
        [SerializeField] private Button _upArrow;
        [SerializeField] private Button _downArrow;
        [SerializeField] private Button _enterWorldButton;
        [SerializeField] private Button _deleteButton;

        private DataRow<WorldCharacterData>[] _characters = new DataRow<WorldCharacterData>[0];
        private Dictionary<Vector2Int, UiPlayerCharacterController> _controllers = new Dictionary<Vector2Int, UiPlayerCharacterController>();

        private int _dataRowPosition = 0;
        private Vector2Int _cursorPosition = Vector2Int.zero;
        private UiPromptWindow _prompt = null;

        void Awake()
        {
            UpdateCharacters();
            _enterWorldButton.interactable = _characters.Length > 0;
            _deleteButton.interactable = _characters.Length > 0;
            SubscribeToMessages();
        }

        public void NewCharacter()
        {
            UiWindowManager.OpenWindow(UiController.CreateCharacter);
            UiWindowManager.CloseWindow(this);
        }

        public void EnterWorld()
        {
            if (_controllers.TryGetValue(_cursorPosition, out var controller))
            {
                DataController.SetCurrentPlayerData(controller.Data);
                ClientController.EnterWorld(controller.Data);
                UiWindowManager.CloseWindow(this);
            }
        }

        public void DeleteCharacter()
        {
            if (!_prompt && _controllers.TryGetValue(_cursorPosition, out var controller))
            {
                _prompt = UiWindowManager.OpenWindow(UiController.Prompt);
                _prompt.Setup($"Are you sure you want to delete {controller.Data.Name}?", DeletePromptResult);
            }
        }

        public void ApplyIndexChange(int change)
        {
            if (change > 0 && _dataRowPosition + 1 < _characters.Length - _maxRows + 1)
            {
                _dataRowPosition++;
                RefreshRows();
            }
            else if (change < 0 && _dataRowPosition > 0)
            {
                _dataRowPosition--;
                RefreshRows();
            }

            //_upArrow.interactable = _dataRowPosition > 0;
            //_downArrow.interactable = _dataRowPosition + 1 < _characters.Length - _maxRows + 1;
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

            if (_characters.Length > 0)
            {
                var startingRow = (_characters.Length - _maxRows) < 0 ? 0 : _dataRowPosition;
                var endingRow = _characters.Length - 1;
                if (_characters.Length > _maxRows)
                {
                    endingRow = Mathf.Min(_dataRowPosition + _maxRows - 1, _characters.Length - 1);
                }


                var position = Vector2Int.zero;
                for (var row = startingRow; row <= endingRow; row++)
                {
                    var itemRow = _characters[row];
                    foreach (var item in itemRow.Items)
                    {
                        var controller = Instantiate(_controllerTemplate, _grid.transform);
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
            else
            {
                _cursor.gameObject.SetActive(false);
            }

            _upArrow.interactable = _dataRowPosition > 0;
            _downArrow.interactable = _dataRowPosition + 1 < _characters.Length - _maxRows + 1;
        }

        private void UpdateCharacters()
        {
            var dataItems = new List<DataRow<WorldCharacterData>>();
            var dataRow = new DataRow<WorldCharacterData>();
            var characters = DataController.AllCharacters;
            if (characters.Length > 0)
            {
                dataItems.Add(dataRow);
                foreach (var character in characters)
                {
                    dataRow.Items.Add(character);
                    if (dataRow.Items.Count >= _grid.constraintCount)
                    {
                        var available = dataItems.Count * _grid.constraintCount;
                        if (available < characters.Length)
                        {
                            dataRow = new DataRow<WorldCharacterData>();
                            dataItems.Add(dataRow);
                        }
                    }
                }
            }

            _characters = dataItems.ToArray();
            _enterWorldButton.interactable = _characters.Length > 0;
            _deleteButton.interactable = _characters.Length > 0;

            RefreshRows();
        }

        private void DeletePromptResult(bool confirm)
        {
            if (confirm)
            {
                if (_controllers.TryGetValue(_cursorPosition, out var controller))
                {
                    DataController.DeleteCharacter(controller.Data);
                }
            }
            StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                UiWindowManager.CloseWindow(_prompt);
                _prompt = null;
            }));
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<SetSelectedPlayerCharacterControllerMessage>(SetSelectedPlayerCharacterController);
            gameObject.Subscribe<WorldCharactersUpdatedMessage>(WorldCharactersUpdated);
        }

        private void SetSelectedPlayerCharacterController(SetSelectedPlayerCharacterControllerMessage msg)
        {
            if (msg.Controller.Position != _cursorPosition)
            {
                if (_controllers.TryGetValue(_cursorPosition, out var currentcontroller))
                {
                    currentcontroller.RemoveCursor();
                }
                _cursorPosition = msg.Controller.Position;
                msg.Controller.SetCursor(_cursor);
            }
        }

        private void WorldCharactersUpdated(WorldCharactersUpdatedMessage msg)
        {
            UpdateCharacters();
        }

        private void UpdateClientState(UpdateClientStateMessage msg)
        {
            if (msg.State == WorldClientState.Disconnected)
            {
                UiWindowManager.CloseWindow(this);
            }
        }
    }
}