using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates
{
    [CreateAssetMenu(fileName = "Server Unit Template", menuName = "Ancible Tools/Server/Unit Templates/Server Unit Template")]
    public class ServerUnitTemplate : ScriptableObject
    {
        public string DisplayName;
        public SpriteTrait Sprite;
        public ServerTrait[] Traits;
        public bool ShowOnClient;
        public bool IsMonster;

        public ObjectSpawnData GetData()
        {
            var data = new ObjectSpawnData
            {
                DisplayName = DisplayName,
                Traits = GetTraits(),
                ShowOnClient = ShowOnClient,
                IsMonster = IsMonster,
                Parameters = new ObjectParameter[0]
            };
            return data;
        }

        public virtual string[] GetTraits()
        {
            var traits = Traits.Where(t => t).Select(t => t.name).ToList();
            if (Sprite)
            {
                traits.Add(Sprite.name);
            }

            return traits.ToArray();
        }
    }
}