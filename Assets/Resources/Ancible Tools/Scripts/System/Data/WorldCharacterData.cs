using System;
using CauldronOnlineCommon.Data;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Data
{
    [Serializable]
    public class WorldCharacterData
    {
        public static string EXTENSION = "wcd";

        public string Name;
        public WorldVector2Int Position;
        public string Zone;
        public CombatStats Stats;
        public CombatVitals Vitals;
        public LoadoutSlotData[] Loadout;
        public string[] EquippedArmor;
        public AspectData[] Aspects;
        public int AvailablePoints;
        public ItemStackData[] Inventory;
        public int Experience;
        public int Level;
        public int Gold;

        public ClientCharacterData ToClientData()
        {
            return new ClientCharacterData
            {
                DisplayName = Name,
                Stats = Stats,
                Vitals = Vitals,
            };
        }
    }
}