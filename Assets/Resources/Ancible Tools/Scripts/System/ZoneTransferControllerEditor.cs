#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [CustomEditor(typeof(ZoneTransferController))]
    public class ZoneTransferControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            if (serializedObject.targetObject is ZoneTransferController zoneTransfer)
            {
                var positionProp = serializedObject.FindProperty("Position");
                if (zoneTransfer.Zone && zoneTransfer.Zone.Controller)
                {
                    var playerSpawns = zoneTransfer.Zone.Controller.GetPlayerSpawns();
                    if (playerSpawns.Length > 0)
                    {
                        var selected = 0;
                        var options = playerSpawns.Select(p => p.DisplayName).ToArray();
                        var current = playerSpawns.FirstOrDefault(p => p.WorldPosition == positionProp.vector2IntValue);
                        if (current)
                        {
                            var index = Array.IndexOf(options, current.DisplayName);
                            if (index >= 0)
                            {
                                selected = index;
                            }
                            else
                            {
                                current = playerSpawns.First(p => p.DisplayName == options[0]);
                            }
                        }
                        var newIndex = EditorGUILayout.Popup("Spawn", selected, options);
                        var selectedOption = playerSpawns.FirstOrDefault(s => s.DisplayName == options[newIndex]);
                        if (selectedOption && (!current || selectedOption != current))
                        {
                            positionProp.vector2IntValue = selectedOption.WorldPosition;
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(positionProp);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(positionProp);
                }
            }
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif
