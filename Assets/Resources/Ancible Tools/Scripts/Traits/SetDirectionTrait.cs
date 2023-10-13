using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Set Direction Trait", menuName = "Ancible Tools/Traits/General/Set Direction")]
    public class SetDirectionTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private Vector2Int _direction = Vector2Int.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
            setDirectionMsg.Direction = _direction;
            _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setDirectionMsg);
        }
    }
}