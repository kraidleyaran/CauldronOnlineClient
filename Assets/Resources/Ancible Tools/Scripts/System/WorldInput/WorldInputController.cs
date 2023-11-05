using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    public class WorldInputController : MonoBehaviour
    {
        public static WorldKeyboardInputLayout KeyboardLayout => _instance._defaultKeyboardLayout;
        public static WorldInputLayoutType CurrentInput => _instance._currentInput;

        private static WorldInputController _instance = null;

        [SerializeField] private Key _devMenu = Key.F8;
        [SerializeField] private WorldKeyboardInputLayout _defaultKeyboardLayout;
        [SerializeField] private WorldGamepadInputLayout _defaultGamepadLayout;
        
        [Header("Gamepad Button Sprites")]
        [SerializeField] private WorldGamepadButtonLayout _xboxLayout;


        private WorldInputState _previous;
        private WorldInputLayoutType _currentInput = WorldInputLayoutType.Keyboard;
        private WorldGamepadType _gamepadType = WorldGamepadType.Generic;
        private bool _gamePadActive = false;


        private UpdateInputStateMessage _updateInputStateMsg = new UpdateInputStateMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _gamePadActive = Gamepad.current != null;
            if (_gamePadActive)
            {
                RefreshGamepadType();
            }
            InputSystem.onDeviceChange += InputSystemOnOnDeviceChange;
            _instance = this;
            SubscribeToMessages();
        }

        private void RefreshGamepadType()
        {
            if (Gamepad.current is DualShockGamepad)
            {
                _gamepadType = WorldGamepadType.Playstation;
            }
            else if (Gamepad.current is XInputController)
            {
                _gamepadType = WorldGamepadType.Xbox;
            }
            //else if (Gamepad.current is SwitchProControllerHID)
            //{
            //    _gamepadType = WorldGamepadType.Switch;
            //}
            else
            {
                _gamepadType = WorldGamepadType.Generic;
            }
        }

        private void InputSystemOnOnDeviceChange(InputDevice arg1, InputDeviceChange arg2)
        {
            switch (arg2)
            {
                case InputDeviceChange.Added:
                    _gamePadActive = Gamepad.current != null;
                    if (_gamePadActive)
                    {
                        RefreshGamepadType();
                    }
                    break;
                case InputDeviceChange.Removed:
                    break;
                case InputDeviceChange.Disconnected:
                    _gamePadActive = Gamepad.current != null;
                    if (_gamePadActive)
                    {
                        RefreshGamepadType();
                    }
                    break;
                case InputDeviceChange.Reconnected:
                    break;
                case InputDeviceChange.Enabled:
                    break;
                case InputDeviceChange.Disabled:
                    break;
                case InputDeviceChange.UsageChanged:
                    break;
                case InputDeviceChange.ConfigurationChanged:
                    break;
                case InputDeviceChange.SoftReset:
                    break;
                case InputDeviceChange.HardReset:
                    break;
            }
        }

        private WorldInputState GetGamepadInput(WorldInputState currentState)
        {
            //currentState.Up = currentState.Up || Gamepad.current.leftStick.up.isPressed;
            //currentState.Down = currentState.Down || Gamepad.current.leftStick.down.isPressed;
            //currentState.Left = currentState.Left || Gamepad.current.leftStick.left.isPressed;
            //currentState.Right = currentState.Right || Gamepad.current.leftStick.right.isPressed;
            //currentState.LeftShoulder = currentState.LeftShoulder || Gamepad.current.leftShoulder.isPressed;
            //currentState.RightShoulder = currentState.RightShoulder || Gamepad.current.rightShoulder.isPressed;
            //currentState.PlayerMenu = currentState.PlayerMenu || Gamepad.current.startButton.isPressed;
            //currentState.LeftTrigger = currentState.LeftTrigger || Gamepad.current.leftTrigger.isPressed;
            //currentState.Green = currentState.Green || Gamepad.current.aButton.isPressed;
            //currentState.Red = currentState.Red || Gamepad.current.bButton.isPressed;
            //currentState.Blue = currentState.Blue || Gamepad.current.xButton.isPressed;
            //currentState.Yellow = currentState.Yellow || Gamepad.current.yButton.isPressed;
            //currentState.Info = currentState.Info || Gamepad.current.selectButton.isPressed;

            //for (var i = 0; i < currentState.Loadout.Length; i++)
            //{
            //    currentState.Loadout[i] = currentState.Loadout[i] || GetGamepadStateForLoadoutSot(i, Gamepad.current);
            //}
            return _defaultGamepadLayout.GetInputState(currentState);
        }

        private bool GetGamepadStateForLoadoutSot(int slot, Gamepad gamepad)
        {
            switch (slot)
            {
                case 0:
                    return gamepad.buttonSouth.isPressed && !gamepad.rightTrigger.isPressed;
                case 1:
                    return gamepad.buttonWest.isPressed && !gamepad.rightTrigger.isPressed;
                case 2:
                    return gamepad.buttonNorth.isPressed && !gamepad.rightTrigger.isPressed;
                case 3:
                    return gamepad.buttonEast.isPressed && !gamepad.rightTrigger.isPressed;
                case 4:
                    return gamepad.buttonSouth.isPressed && gamepad.rightTrigger.isPressed;
                case 5:
                    return gamepad.buttonWest.isPressed && gamepad.rightTrigger.isPressed;
                case 6:
                    return gamepad.buttonNorth.isPressed && gamepad.rightTrigger.isPressed;
                case 7:
                    return gamepad.buttonEast.isPressed && gamepad.rightTrigger.isPressed;
                default:
                    return false;

            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_defaultKeyboardLayout.GetInputState(new WorldInputState{Loadout = new bool[8]}).AnyInput && _currentInput == WorldInputLayoutType.Gamepad)
            {
                _currentInput = WorldInputLayoutType.Keyboard;
                gameObject.SendMessage(InputTypeUpdatedMessage.INSTANCE);
            }
            else if (Gamepad.current != null && _defaultGamepadLayout.GetInputState(new WorldInputState{Loadout = new bool[8]}).AnyInput && _currentInput == WorldInputLayoutType.Keyboard)
            {
                //Debug.Log("Gamepad input updated");
                _currentInput = WorldInputLayoutType.Gamepad;
                gameObject.SendMessage(InputTypeUpdatedMessage.INSTANCE);
            }
            
            var state = new WorldInputState {Loadout = new bool[8]};
            var current = _defaultKeyboardLayout.GetInputState(state);
            if (Debug.isDebugBuild)
            {
                current.DevWindow = Keyboard.current[_devMenu].isPressed;
            }
            if (Gamepad.current != null)
            {
                current = GetGamepadInput(current);
            }

            _updateInputStateMsg.Current = current;
            _updateInputStateMsg.Previous = _previous;
            gameObject.SendMessage(_updateInputStateMsg);
            _previous = current;
        }

        public static Sprite GetGamepadButtonSpriteByGamepadInputType(WorldGamepadInputType type)
        {
            return _instance._xboxLayout.GetSpriteByButtonType(type);
        }

        public static Sprite GetGamepadButtonSpriteByInputType(WorldInputType type, int index = 0)
        {
            return GetGamepadButtonSpriteByGamepadInputType(_instance._defaultGamepadLayout.GetGamepadInputFromWorldInput(type, index));
        }


    }
}