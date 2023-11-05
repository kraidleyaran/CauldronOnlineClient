using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldInput
{
    [Serializable]
    public class WorldInputLayout
    {
        public virtual WorldInputState GetInputState(WorldInputState current)
        {
            return current;
        }

        public virtual string GetInputStringFromType(WorldInputType type, int index = 0)
        {
            return $"{type}";
        }
    }
}