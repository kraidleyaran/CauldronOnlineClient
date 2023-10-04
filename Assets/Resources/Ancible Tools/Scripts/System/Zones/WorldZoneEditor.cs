#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Zones
{
    [CustomEditor(typeof(WorldZone))]
    public class WorldZoneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            if (serializedObject.targetObject is WorldZone zone)
            {
                var defaultSpawnProp = serializedObject.FindProperty("DefaultSpawn");
                if (zone.Controller)
                {
                    var playerSpawns = zone.Controller.GetPlayerSpawns();
                    if (playerSpawns.Length > 0)
                    {
                        var selected = 0;
                        var options = playerSpawns.Select(p => p.DisplayName).OrderBy(n => n).ToArray();
                        var current = playerSpawns.FirstOrDefault(p => p.WorldPosition == zone.DefaultSpawn);
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
                                defaultSpawnProp.vector2IntValue = current.WorldPosition;
                            }
                        }
                        var newIndex = EditorGUILayout.Popup("Default Spawn", selected, options);
                        var selectedOption = playerSpawns.FirstOrDefault(s => s.DisplayName == options[newIndex]);
                        if (selectedOption && (!current || selectedOption != current))
                        {
                            defaultSpawnProp.vector2IntValue = selectedOption.WorldPosition;
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(defaultSpawnProp);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(defaultSpawnProp);
                }
                

            }
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif