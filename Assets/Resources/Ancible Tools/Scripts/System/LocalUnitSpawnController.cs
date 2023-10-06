using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class LocalUnitSpawnController : MonoBehaviour
    {
        [SerializeField] private UnitTemplate _template;
        [SerializeField] private Trait[] _additionalTraits;

        void Awake()
        {
            var worldPosition = transform.position.ToVector2().ToWorldPosition();
            var unitController = _template ? _template.GenerateUnit(worldPosition.ToWorldVector()) : Instantiate(FactoryController.UNIT_CONTROLLER, worldPosition.ToWorldVector(), Quaternion.identity);
            if (_additionalTraits.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _additionalTraits)
                {
                    addTraitToUnitMsg.Trait = trait;
                    gameObject.SendMessageTo(addTraitToUnitMsg, unitController.gameObject);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }

            var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
            setWorldPositionMsg.Position = worldPosition;
            gameObject.SendMessageTo(setWorldPositionMsg, unitController.gameObject);
            MessageFactory.CacheMessage(setWorldPositionMsg);

            ObjectManager.RegisterObject(unitController.gameObject);
        }
    }
}