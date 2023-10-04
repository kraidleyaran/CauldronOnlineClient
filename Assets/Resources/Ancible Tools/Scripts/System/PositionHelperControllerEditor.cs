#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [CustomEditor(typeof(PositionHelperController))]
    public class PositionHelperControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            if (serializedObject.targetObject is PositionHelperController positionHelper)
            {
                if (positionHelper.IsPlayerSpawn)
                {
                    var displayNameProp = serializedObject.FindProperty("DisplayName");
                    EditorGUILayout.PropertyField(displayNameProp);
                }
                positionHelper.RefreshPositions();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif