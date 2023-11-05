using System.Linq;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    public struct WorldInputState
    {
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
        public bool[] Loadout;
        public bool LeftShoulder;
        public bool RightShoulder;
        public bool PlayerMenu;
        public bool LeftTrigger;
        public bool Green;
        public bool Yellow;
        public bool Red;
        public bool Blue;
        public bool Info;
        public bool DevWindow;

        public bool AnyInput => Up || Down || Left || Right || Loadout.Contains(true) || LeftShoulder ||
                                RightShoulder || PlayerMenu || LeftTrigger || Green || Yellow || Red || Blue || Info;

    }
}