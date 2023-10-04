using System.Collections.Generic;
using System.IO;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Traits;
using CauldronOnlineCommon.Data.TriggerEvents;
using CauldronOnlineCommon.Data.Zones;
using FileDataLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server
{
    [CreateAssetMenu(fileName = "Server Export Settings", menuName = "Ancible Tools/Server/Export Settings")]
    public class ServerExportSettings : ScriptableObject
    {
        [Header("Zones")]
        [SerializeField] private string _internalZoneFolderPath = string.Empty;
        [SerializeField] private string _zoneSaveFolderPath = string.Empty;
        [Header("Traits")]
        [SerializeField] private string[] _internalSpriteTraitPaths = new string[0];
        [SerializeField] private string[] _internalTraitPaths = new string[0];
        [SerializeField] private string _traitSaveFolderPath = string.Empty;
        [Header("Loot")]
        [SerializeField] private string _internalLootTablePath = string.Empty;
        [SerializeField] private string _lootTableSaveFolderPath = string.Empty;
        [Header("Trigger Events")]
        [SerializeField] private string _internalTriggerEventsPath = string.Empty;
        [SerializeField] private string _triggerEventsSaveFolderPath = string.Empty;

        public void Export()
        {
            SaveZones();
            SaveTraits();
            SaveLootTables();
            SaveTriggerEvents();
        }

        private void ClearDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, $"*.*");
                foreach (var file in files)
                {
                    FileData.DeleteFile(file);
                }
            }
        }

        private void SaveZones()
        {
            ClearDirectory(_zoneSaveFolderPath);
            var zones = UnityEngine.Resources.LoadAll<WorldZone>(_internalZoneFolderPath);

            var success = 0;
            foreach (var zone in zones)
            {
                var path = $"{_zoneSaveFolderPath}{Path.DirectorySeparatorChar}{zone.name}.{WorldZoneData.EXTENSION}";
                var result = FileData.SaveData(path, zone.GetData());
                if (result.Success)
                {
                    success++;
                }
                else
                {
                    Debug.LogWarning($"Error while saving {zone.name} to path {path} - {(result.HasException ? $"{result.Exception}" : "Unknown error")}");
                }
            }

            Debug.Log($"Saved {success} of {zones.Length} World Zones");
        }

        private void SaveTraits()
        {
            ClearDirectory(_traitSaveFolderPath);
            var traits = new List<WorldTraitData>();
            foreach (var path in _internalSpriteTraitPaths)
            {
                var spriteTraits = UnityEngine.Resources.LoadAll<SpriteTrait>(path);
                foreach (var trait in spriteTraits)
                {
                    traits.Add(new SpriteTraitData{Name = trait.name, MaxStack = 1, Sprite = trait.name});
                }
            }

            foreach (var path in _internalTraitPaths)
            {
                var serverTraits = UnityEngine.Resources.LoadAll<ServerTrait>(path);
                foreach (var trait in serverTraits)
                {
                    traits.Add(trait.GetData());
                }
            }

            var success = 0;
            foreach (var trait in traits)
            {
                var path = $"{_traitSaveFolderPath}{Path.DirectorySeparatorChar}{trait.Name}.{WorldTraitData.EXTENSION}";
                var result = FileData.SaveData(path, trait);
                if (result.Success)
                {
                    success++;
                }
                else
                {
                    Debug.LogWarning($"Failed to save trait {trait.Name} to path {path} - {(result.HasException ? $"{result.Exception}" : "Unknown error")}");
                }
            }

            Debug.Log($"Saved {success} of {traits.Count} Server Traits");
        }

        private void SaveLootTables()
        {
            ClearDirectory(_lootTableSaveFolderPath);
            var lootTables = UnityEngine.Resources.LoadAll<LootTable>(_internalLootTablePath);

            var success = 0;
            foreach (var table in lootTables)
            {
                var path = $"{_lootTableSaveFolderPath}{Path.DirectorySeparatorChar}{table.name}.{LootTableData.EXTENSION}";
                var result = FileData.SaveData(path, table.GetData());
                if (result.Success)
                {
                    success++;
                }
                else
                {
                    Debug.LogWarning($"Error while saving loot table {table.name} to path {path} - {(result.HasException ? $"{result.Exception}" : "Unknown error")}");
                }
            }

            Debug.Log($"Saved {success} of {lootTables.Length} Loot Tables");
        }

        private void SaveTriggerEvents()
        {
            ClearDirectory(_triggerEventsSaveFolderPath);
            var triggerEvents = UnityEngine.Resources.LoadAll<TriggerEvent>(_internalTriggerEventsPath);

            var success = 0;
            foreach (var triggerEvent in triggerEvents)
            {
                var path = $"{_triggerEventsSaveFolderPath}{Path.DirectorySeparatorChar}{triggerEvent.name}.{TriggerEventData.EXTENSION}";
                var result = FileData.SaveData(path, triggerEvent.GetData());
                if (result.Success)
                {
                    success++;
                }
                else
                {
                    Debug.LogWarning($"Error while saving trigger event {triggerEvent.name} to path {path} - {(result.HasException ? $"{result.Exception}" : "Unknown error")}");
                }
            }

            Debug.Log($"Saved {success} of {triggerEvents.Length} Trigger Events");
        }
    }
}