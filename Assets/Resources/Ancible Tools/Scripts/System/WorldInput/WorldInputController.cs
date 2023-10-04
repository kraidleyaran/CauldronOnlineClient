using MessageBusLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    public class WorldInputController : MonoBehaviour
    {
        private static WorldInputController _instance = null;

        [SerializeField] private Key _up = Key.UpArrow;
        [SerializeField] private Key _down = Key.DownArrow;
        [SerializeField] private Key _left = Key.LeftArrow;
        [SerializeField] private Key _right = Key.RightArrow;
        [SerializeField] private Key[] _loadout = {Key.Z, Key.X, Key.C, Key.V, Key.A, Key.S, Key.D, Key.F};
        [SerializeField] private Key _menuLeft = Key.Q;
        [SerializeField] private Key _menuRight = Key.E;
        [SerializeField] private Key _playerMenu = Key.P;
        [SerializeField] private Key _leftTrigger = Key.LeftShift;
        [SerializeField] private Key _green = Key.Space;
        [SerializeField] private Key _red = Key.C;
        [SerializeField] private Key _blue = Key.Z;
        [SerializeField] private Key _yellow = Key.X;
        [SerializeField] private Key _info = Key.I;

        private WorldInputState _previous;

        private UpdateInputStateMessage _updateInputStateMsg = new UpdateInputStateMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        private WorldInputState GetGamepadInput(WorldInputState currentState)
        {
            currentState.Up = currentState.Up || Gamepad.current.leftStick.up.isPressed;
            currentState.Down = currentState.Down || Gamepad.current.leftStick.down.isPressed;
            currentState.Left = currentState.Left || Gamepad.current.leftStick.left.isPressed;
            currentState.Right = currentState.Right || Gamepad.current.leftStick.right.isPressed;
            currentState.LeftShoulder = currentState.LeftShoulder || Gamepad.current.leftShoulder.isPressed;
            currentState.RightShoulder = currentState.RightShoulder || Gamepad.current.leftShoulder.isPressed;
            currentState.PlayerMenu = currentState.PlayerMenu || Gamepad.current.startButton.isPressed;
            currentState.LeftTrigger = currentState.LeftTrigger || Gamepad.current.leftTrigger.isPressed;
            currentState.Green = currentState.Green || Gamepad.current.buttonSouth.isPressed;
            currentState.Red = currentState.Red || Gamepad.current.buttonEast.isPressed;
            currentState.Blue = currentState.Blue || Gamepad.current.buttonWest.isPressed;
            currentState.Yellow = currentState.Yellow || Gamepad.current.buttonNorth.isPressed;
            currentState.Info = currentState.Info || Gamepad.current.selectButton.isPressed;

            for (var i = 0; i < currentState.Loadout.Length; i++)
            {
                currentState.Loadout[i] = currentState.Loadout[i] || GetGamepadStateForLoadoutSot(i, Gamepad.current);
            }
            return currentState;
        }

        private bool[] GetLoadoutInputState()
        {
            var returnValue = new bool[_loadout.Length];
            for (var i = 0; i < returnValue.Length; i++)
            {
                returnValue[i] = Keyboard.current[_loadout[i]].isPressed;
            }

            return returnValue;
        }

        private bool GetGamepadStateForLoadoutSot(int slot, Gamepad gamepad)
        {
            switch (slot)
            {
                case 0:
                    return gamepad.buttonSouth.isPressed;
                case 1:
                    return gamepad.buttonWest.isPressed;
                case 2:
                    return gamepad.buttonNorth.isPressed;
                case 3:
                    return gamepad.buttonEast.isPressed;
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
            var current = new WorldInputState
            {
                Up = Keyboard.current[_up].isPressed,
                Down = Keyboard.current[_down].isPressed,
                Left = Keyboard.current[_left].isPressed,
                Right = Keyboard.current[_right].isPressed,
                Loadout =  GetLoadoutInputState(),
                LeftShoulder = Keyboard.current[_menuLeft].isPressed,
                RightShoulder = Keyboard.current[_menuRight].isPressed,
                PlayerMenu =  Keyboard.current[_playerMenu].isPressed,
                LeftTrigger = Keyboard.current[_leftTrigger].isPressed,
                Green = Keyboard.current[_green].isPressed,
                Red = Keyboard.current[_red].isPressed,
                Blue = Keyboard.current[_blue].isPressed,
                Yellow = Keyboard.current[_yellow].isPressed,
                Info = Keyboard.current[_info].isPressed
            };

            if (Gamepad.current != null)
            {
                current = GetGamepadInput(current);
            }
            _updateInputStateMsg.Current = current;
            _updateInputStateMsg.Previous = _previous;
            gameObject.SendMessage(_updateInputStateMsg);
            _previous = current;
        }
    }
}