using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Input Movement Trait", menuName = "Ancible Tools/Traits/Player/Input Movement")]
    public class InputMovementTrait : Trait
    {
        private Vector2Int _direction = Vector2Int.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateDirectionMessage>(UpdateDirection, _instanceId);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            var direction = Vector2Int.zero;
            if (msg.Current.Up)
            {
                direction.y = 1;
            }
            else if (msg.Current.Down)
            {
                direction.y = -1;
            }

            if (msg.Current.Left)
            {
                direction.x = -1;
            }
            else if (msg.Current.Right)
            {
                direction.x = 1;
            }

            if (direction != _direction)
            {
                var setDirectionMsg = MessageFactory.GenerateSetDirectionMsg();
                setDirectionMsg.Direction = direction;
                _controller.gameObject.SendMessageTo(setDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setDirectionMsg);
            }

        }

        private void UpdateDirection(UpdateDirectionMessage msg)
        {
            _direction = msg.Direction;
        }
    }
}