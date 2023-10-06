using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Templates
{
    [CreateAssetMenu(fileName = "Extended Unit Template", menuName = "Ancible Tools/Templates/Extended Unit Template")]
    public class ExtendedUnitTemplate : UnitTemplate
    {
        [SerializeField] private UnitTemplate _baseTemplate;

        public override Trait[] GetTraits()
        {
            var traits = _baseTemplate.GetTraits().ToList();
            traits.AddRange(_traits);
            return traits.ToArray();
        }

        public override UnitController GenerateUnit(Vector2 position)
        {
            var traits = GetTraits();
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, position, Quaternion.identity);
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }

        public override UnitController GenerateUnitWithRotation(Vector2 position, float rotation)
        {
            var traits = GetTraits();
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, position, Quaternion.Euler(0, 0, rotation));
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }
    }
}