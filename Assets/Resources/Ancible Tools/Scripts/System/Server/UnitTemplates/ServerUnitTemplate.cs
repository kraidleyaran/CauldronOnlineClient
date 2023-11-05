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
        public bool ShowName;
        public bool ShowAppearance = true;

        public ObjectSpawnData GetData(bool includeSprite = true)
        {
            var data = new ObjectSpawnData
            {
                DisplayName = DisplayName,
                Traits = GetTraits(includeSprite),
                ShowOnClient = ShowOnClient,
                IsMonster = IsMonster,
                ShowNameOnClient = ShowName,
                Parameters = new ObjectParameter[0],
                ShowAppearance = ShowAppearance
            };
            return data;
        }

        public virtual string[] GetTraits(bool includeSprite = true)
        {
            var traits = Traits.Where(t => t).Select(t => t.name).ToList();
            if (includeSprite && Sprite)
            {
                traits.Add(Sprite.name);
            }

            return traits.ToArray();
        }

        public virtual SpriteTrait GetSprite()
        {
            return Sprite;
        }
    }
}