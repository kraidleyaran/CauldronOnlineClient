using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    [Serializable]
    public class WorldGamepadInputLayout : WorldInputLayout
    {
        public WorldGamepadInputType Up = WorldGamepadInputType.LStickUp;
        public WorldGamepadInputType Down = WorldGamepadInputType.LStickDown;
        public WorldGamepadInputType Left = WorldGamepadInputType.LStickLeft;
        public WorldGamepadInputType Right = WorldGamepadInputType.LStickRight;
        public WorldGamepadInputType[] Loadout = {
            WorldGamepadInputType.A,
            WorldGamepadInputType.B,
            WorldGamepadInputType.X,
            WorldGamepadInputType.Y,
        };

        public WorldGamepadInputType MenuLeft = WorldGamepadInputType.LeftShoulder;
        public WorldGamepadInputType MenuRight = WorldGamepadInputType.RightShoulder;
        public WorldGamepadInputType PlayerMenu = WorldGamepadInputType.Start;
        public WorldGamepadInputType LeftTrigger = WorldGamepadInputType.LeftTrigger;
        public WorldGamepadInputType Green = WorldGamepadInputType.A;
        public WorldGamepadInputType Red = WorldGamepadInputType.B;
        public WorldGamepadInputType Blue = WorldGamepadInputType.X;
        public WorldGamepadInputType Yellow = WorldGamepadInputType.Y;
        public WorldGamepadInputType Info = WorldGamepadInputType.Select;
        public WorldGamepadInputType Mod = WorldGamepadInputType.RightTrigger;

        public override WorldInputState GetInputState(WorldInputState current)
        {
            if (Gamepad.current != null)
            {
                var gamepad = Gamepad.current;
                current.Up = current.Up || Up.ToInputState(gamepad);
                current.Down = current.Down || Down.ToInputState(gamepad);
                current.Left = current.Left || Left.ToInputState(gamepad);
                current.Right = current.Right || Right.ToInputState(gamepad);
                current.Loadout = GetLoadout(current.Loadout);
                current.LeftShoulder = current.LeftShoulder || MenuLeft.ToInputState(gamepad);
                current.RightShoulder = current.RightShoulder || MenuRight.ToInputState(gamepad);
                current.Green = current.Green || Green.ToInputState(gamepad);
                current.Red = current.Red || Red.ToInputState(gamepad);
                current.Yellow = current.Yellow || Yellow.ToInputState(gamepad);
                current.Blue = current.Blue || Blue.ToInputState(gamepad);
                current.Info = current.Info || Info.ToInputState(gamepad);
                current.LeftTrigger = current.LeftTrigger || LeftTrigger.ToInputState(gamepad);
                current.PlayerMenu = current.PlayerMenu || PlayerMenu.ToInputState(gamepad);
            }

            return current;
        }

        private bool[] GetLoadout(bool[] current)
        {
            current[0] = current[0] || Loadout[0].ToInputState(Gamepad.current) && !Mod.ToInputState(Gamepad.current);
            current[1] = current[1] || Loadout[1].ToInputState(Gamepad.current) && !Mod.ToInputState(Gamepad.current); ;
            current[2] = current[2] || Loadout[2].ToInputState(Gamepad.current) && !Mod.ToInputState(Gamepad.current); ;
            current[3] = current[3] || Loadout[3].ToInputState(Gamepad.current) && !Mod.ToInputState(Gamepad.current); ;
            current[4] = current[4] || Loadout[0].ToInputState(Gamepad.current) && Mod.ToInputState(Gamepad.current); ;
            current[5] = current[5] || Loadout[1].ToInputState(Gamepad.current) && Mod.ToInputState(Gamepad.current);
            current[6] = current[6] || Loadout[2].ToInputState(Gamepad.current) && Mod.ToInputState(Gamepad.current);
            current[7] = current[7] || Loadout[3].ToInputState(Gamepad.current) && Mod.ToInputState(Gamepad.current);

            return current;
        }

        public WorldGamepadInputType GetGamepadInputFromWorldInput(WorldInputType type, int index = 0)
        {
            switch (type)
            {
                case WorldInputType.Up:
                    return Up;
                case WorldInputType.Down:
                    return Down;
                case WorldInputType.Left:
                    return Left;
                case WorldInputType.Right:
                    return Right;
                case WorldInputType.Loadout:
                    var loadoutIndex = index;
                    if (loadoutIndex > 3)
                    {
                        loadoutIndex -= 4;
                    }

                    if (loadoutIndex < Loadout.Length)
                    {
                        return Loadout[loadoutIndex];
                    }
                    else
                    {
                        return Loadout[0];
                    }
                case WorldInputType.PlayerMenu:
                    return PlayerMenu;
                case WorldInputType.MenuLeft:
                    return MenuLeft;
                case WorldInputType.MenuRight:
                    return MenuRight;
                case WorldInputType.Mod:
                    return Mod;
                case WorldInputType.Green:
                    return Green;
                case WorldInputType.Yellow:
                    return Yellow;
                case WorldInputType.Red:
                    return Red;
                case WorldInputType.Blue:
                    return Blue;
                case WorldInputType.Info:
                    return Info;
                default:
                    return Green;
            }
        }
    }
}