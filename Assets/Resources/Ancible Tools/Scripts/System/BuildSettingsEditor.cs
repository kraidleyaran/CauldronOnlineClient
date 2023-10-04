#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Build
{
    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            if (serializedObject.targetObject is BuildSettings buildSettings && GUILayout.Button("Build Dev"))
            {
                var buildProperty = serializedObject.FindProperty("Build");
                if (buildProperty != null)
                {
                    buildProperty.intValue = buildProperty.intValue + 1;
                    serializedObject.ApplyModifiedProperties();
                }
                buildSettings.BuildDev();
            }
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif