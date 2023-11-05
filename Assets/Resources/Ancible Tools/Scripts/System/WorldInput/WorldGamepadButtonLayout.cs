using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    [CreateAssetMenu(fileName = "World Gamepad Button Layout", menuName = "Ancible Tools/Input/World Gamepad Button Layout")]
    public class WorldGamepadButtonLayout : ScriptableObject
    {
        [SerializeField] private Sprite _leftStick;
        [SerializeField] private Sprite _rightStick;
        [SerializeField] private Sprite _aButton;
        [SerializeField] private Sprite _bButton;
        [SerializeField] private Sprite _xButton;
        [SerializeField] private Sprite _yButton;
        [SerializeField] private Sprite _dpadUp;
        [SerializeField] private Sprite _dpadDown;
        [SerializeField] private Sprite _dpadLeft;
        [SerializeField] private Sprite _dpadRight;
        [SerializeField] private Sprite _leftShoulder;
        [SerializeField] private Sprite _rightShoulder;
        [SerializeField] private Sprite _leftTrigger;
        [SerializeField] private Sprite _rightTrigger;
        [SerializeField] private Sprite _start;
        [SerializeField] private Sprite _select;

        public Sprite GetSpriteByButtonType(WorldGamepadInputType type)
        {
            switch (type)
            {
                case WorldGamepadInputType.LStickUp:
                case WorldGamepadInputType.LStickDown:
                case WorldGamepadInputType.LStickLeft:
                case WorldGamepadInputType.LStickRight:
                    return _leftStick;
                case WorldGamepadInputType.RStickUp:
                case WorldGamepadInputType.RStickDown:
                case WorldGamepadInputType.RStickLeft:
                case WorldGamepadInputType.RStickRight:
                    return _rightStick;
                case WorldGamepadInputType.A:
                    return _aButton;
                case WorldGamepadInputType.B:
                    return _bButton;
                case WorldGamepadInputType.X:
                    return _xButton;
                case WorldGamepadInputType.Y:
                    return _yButton;
                case WorldGamepadInputType.Start:
                    return _start;
                case WorldGamepadInputType.Select:
                    return _select;
                case WorldGamepadInputType.LeftShoulder:
                    return _leftShoulder;
                case WorldGamepadInputType.RightShoulder:
                    return _rightShoulder;
                case WorldGamepadInputType.LeftTrigger:
                    return _leftTrigger;
                case WorldGamepadInputType.RightTrigger:
                    return _rightTrigger;
                case WorldGamepadInputType.DpadUp:
                    return _dpadUp;
                case WorldGamepadInputType.DpadDown:
                    return _dpadDown;
                case WorldGamepadInputType.DpadLeft:
                    return _dpadLeft;
                case WorldGamepadInputType.DpadRight:
                    return _dpadRight;
                default:
                    return null;
            }
        }

    }
}