using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates
{
    [CreateAssetMenu(fileName = "Extended Server Unit Template", menuName = "Ancible Tools/Server/Unit Templates/Extended Server Unit Template")]
    public class ExtendedServerUnitTemplate : ServerUnitTemplate
    {
        [SerializeField] private ServerUnitTemplate _baseTemplate;

        public override string[] GetTraits()
        {
            var traits = _baseTemplate.GetTraits().ToList();
            if (Sprite)
            {
                if (_baseTemplate.Sprite)
                {
                    traits.Remove(_baseTemplate.Sprite.name);
                }
                traits.Add(Sprite.name);
            }
            traits.AddRange(Traits.Where(t => t).Select(t => t.name));
            return traits.ToArray();
        }
    }
}