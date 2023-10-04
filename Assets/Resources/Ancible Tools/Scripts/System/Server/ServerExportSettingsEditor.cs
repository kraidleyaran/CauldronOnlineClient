#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server
{
    [CustomEditor(typeof(ServerExportSettings))]
    public class ServerExportSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            if (GUILayout.Button("Export") && serializedObject.targetObject is ServerExportSettings exportSettings)
            {
                exportSettings.Export();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
