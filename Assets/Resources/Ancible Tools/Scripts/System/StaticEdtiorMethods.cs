#if UNITY_EDITOR
using UnityEditor;
namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public static class StaticEdtiorMethods
    {
        public static string GenerateId()
        {
            return GUID.Generate().ToString();
        }
    }
}
#endif
