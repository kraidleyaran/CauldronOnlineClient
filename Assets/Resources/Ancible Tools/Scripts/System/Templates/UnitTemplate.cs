using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Templates
{
    [CreateAssetMenu(fileName = "Unit Template", menuName = "Ancible Tools/Templates/Unit Template")]
    public class UnitTemplate : ScriptableObject
    {
        [SerializeField] protected internal Trait[] _traits = new Trait[0];

        public virtual UnitController GenerateUnit(Vector2 position)
        {
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, position, Quaternion.identity);
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var traits = GetTraits();
            for (var i = 0; i < traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }

        public virtual UnitController GenerateUnitWithRotation(Vector2 position, float rotation)
        {
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, position, Quaternion.Euler(0,0,rotation));
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var traits = GetTraits();
            for (var i = 0; i < traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }

        public virtual UnitController GenerateUnitWithParent(Transform parent)
        {
            var unitController = Instantiate(FactoryController.UNIT_CONTROLLER, parent);
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var traits = GetTraits();
            for (var i = 0; i < traits.Length; i++)
            {
                addTraitToUnitMsg.Trait = traits[i];
                this.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
            return unitController;
        }

        public virtual Trait[] GetTraits()
        {
            return _traits.ToArray();
        }
    }
}