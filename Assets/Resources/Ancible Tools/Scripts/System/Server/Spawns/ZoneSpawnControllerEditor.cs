#if UNITY_EDITOR
using UnityEditor;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    [CustomEditor(typeof(ZoneSpawnController))]
    public class ZoneSpawnControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            if (serializedObject.targetObject is ZoneSpawnController spawn)
            {
                spawn.RefreshEditorSprite();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
