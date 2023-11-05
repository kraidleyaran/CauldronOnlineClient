using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldInput;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.WorldInput
{
    public class UiWorldInputButtonController : MonoBehaviour
    {
        [SerializeField] private Sprite _defaultKeySprite;
        [SerializeField] private WorldInputType _inputType;
        [SerializeField] private int _index = 0;
        [SerializeField] private Image _keyboardButtonImage;
        [SerializeField] private Text _keyboardButtonText;
        [SerializeField] private GameObject _gamePadGroup;
        [SerializeField] private Image _gamepadButtonImage;
        [SerializeField] private Image _gamepadModButtonImage;
        [SerializeField] private Text _gamepadPlusText;
        

        void Awake()
        {
            RefreshInput();
            SubscribeToMessages();
        }

        private void RefreshInput()
        {
            switch (WorldInputController.CurrentInput)
            {
                case WorldInputLayoutType.Keyboard:
                    _keyboardButtonText.text = WorldInputController.KeyboardLayout.GetInputStringFromType(_inputType, _index);
                    _keyboardButtonImage.gameObject.SetActive(true);
                    _gamePadGroup.gameObject.SetActive(false);
                    break;
                case WorldInputLayoutType.Gamepad:
                    _keyboardButtonText.text = string.Empty;
                    _keyboardButtonImage.gameObject.SetActive(false);
                    _gamePadGroup.gameObject.SetActive(true);
                    _gamepadButtonImage.sprite = WorldInputController.GetGamepadButtonSpriteByInputType(_inputType, _index);
                    if (_index > 3)
                    {
                        _gamepadModButtonImage.sprite = WorldInputController.GetGamepadButtonSpriteByInputType(WorldInputType.Mod);
                        _gamepadModButtonImage.gameObject.SetActive(true);
                        _gamepadPlusText.gameObject.SetActive(true);
                    }
                    else
                    {
                        _gamepadPlusText.gameObject.SetActive(false);
                        _gamepadModButtonImage.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<InputTypeUpdatedMessage>(InputTypeUpdated);
        }

        private void InputTypeUpdated(InputTypeUpdatedMessage msg)
        {
            RefreshInput();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}