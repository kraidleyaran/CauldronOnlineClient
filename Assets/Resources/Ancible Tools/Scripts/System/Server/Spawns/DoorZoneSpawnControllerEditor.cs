#if UNITY_EDITOR
using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using UnityEditor;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    [CustomEditor(typeof(DoorZoneSpawnController))]
    public class DoorZoneSpawnControllerEditor : ZoneSpawnControllerEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (serializedObject.targetObject is DoorZoneSpawnController zoneTransfer)
            {
                var worldZoneController = zoneTransfer.GetComponentInParent<WorldZoneController>();
                
                var positionProp = serializedObject.FindProperty("TrappedSpawnPosition");
                if (worldZoneController)
                {
                    var playerSpawns = worldZoneController.GetPlayerSpawns();
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
                        if (!selectedOption)
                        {
                            selectedOption = playerSpawns[0];
                        }
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

                serializedObject.ApplyModifiedProperties();
            }

        }
    }
}
#endif
