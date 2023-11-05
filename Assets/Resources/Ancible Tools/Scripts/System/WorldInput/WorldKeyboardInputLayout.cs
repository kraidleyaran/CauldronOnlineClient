using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    [Serializable]
    public class WorldKeyboardInputLayout : WorldInputLayout
    {
        public const string UP = "Up";
        public const string DOWN = "Down";
        public const string LEFT = "Left";
        public const string RIGHT = "Right";
        public const string LEFT_SHIFT = "L-Shft";
        public const string RIGHT_SHIFT = "R-Shft";
        public const string LEFT_CONTROL = "L-Ctrl";
        public const string RIGHT_CONTROL = "R-Ctrl";
        public const string LEFT_ALT = "L-Alt";
        public const string RIGHT_ALT = "R-Alt";

        public Key Up = Key.UpArrow;
        public Key Down = Key.DownArrow;
        public Key Left = Key.LeftArrow;
        public Key Right = Key.RightArrow;
        public Key[] Loadout = { Key.Z, Key.X, Key.C, Key.V, Key.A, Key.S, Key.D, Key.F };
        public Key MenuLeft = Key.Q;
        public Key MenuRight = Key.E;
        public Key PlayerMenu = Key.P;
        public Key LeftTrigger = Key.LeftShift;
        public Key Green = Key.Space;
        public Key Red = Key.C;
        public Key Blue = Key.Z;
        public Key Yellow = Key.X;
        public Key Info = Key.I;

        public override WorldInputState GetInputState(WorldInputState current)
        {
            current.Up = current.Up || Keyboard.current[Up].isPressed;
            current.Down = current.Down || Keyboard.current[Down].isPressed;
            current.Left = current.Left || Keyboard.current[Left].isPressed;
            current.Right = current.Right || Keyboard.current[Right].isPressed;
            current.Loadout = GetLoadoutInputState(current.Loadout);
            current.LeftShoulder = current.LeftShoulder || Keyboard.current[MenuLeft].isPressed;
            current.RightShoulder = current.RightShoulder || Keyboard.current[MenuRight].isPressed;
            current.PlayerMenu = current.PlayerMenu || Keyboard.current[PlayerMenu].isPressed;
            current.LeftTrigger = current.LeftTrigger || Keyboard.current[LeftTrigger].isPressed;
            current.Green = current.Green || Keyboard.current[Green].isPressed;
            current.Red = current.Red || Keyboard.current[Red].isPressed;
            current.Blue = current.Blue || Keyboard.current[Blue].isPressed;
            current.Yellow = current.Yellow || Keyboard.current[Yellow].isPressed;
            current.Info = current.Info || Keyboard.current[Info].isPressed;

            return current;
        }

        public override string GetInputStringFromType(WorldInputType type, int index = 0)
        {
            switch (type)
            {
                case WorldInputType.Up:
                    return Up.ToInputString();
                case WorldInputType.Down:
                    return Down.ToInputString();
                case WorldInputType.Left:
                    return Left.ToInputString();
                case WorldInputType.Right:
                    return Right.ToInputString();
                case WorldInputType.Loadout:
                    if (Loadout.Length > index)
                    {
                        return Loadout[index].ToInputString();
                    }
                    else
                    {
                        return Loadout[0].ToInputString();
                    }
                case WorldInputType.PlayerMenu:
                    return PlayerMenu.ToInputString();
                case WorldInputType.MenuLeft:
                    return MenuLeft.ToInputString();
                case WorldInputType.MenuRight:
                    return MenuRight.ToInputString();
                case WorldInputType.Mod:
                    return string.Empty;
                case WorldInputType.Green:
                    return Green.ToInputString();
                case WorldInputType.Yellow:
                    return Yellow.ToInputString();
                case WorldInputType.Red:
                    return Red.ToInputString();
                case WorldInputType.Blue:
                    return Blue.ToInputString();
                case WorldInputType.Info:
                    return Info.ToInputString();
            }

            return base.GetInputStringFromType(type, index);
        }

        private bool[] GetLoadoutInputState(bool[] current)
        {
            var returnValue = new bool[Loadout.Length];
            var max = Mathf.Min(returnValue.Length, current.Length);
            for (var i = 0; i < max; i++)
            {
                returnValue[i] = current[i] || Keyboard.current[Loadout[i]].isPressed;
            }

            return returnValue;
        }


    }
}