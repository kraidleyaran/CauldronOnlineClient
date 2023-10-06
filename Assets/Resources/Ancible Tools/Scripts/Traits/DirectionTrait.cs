using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Direction Trait", menuName = "Ancible Tools/Traits/General/Direction")]
    public class DirectionTrait : Trait
    {
        private Vector2Int _direction = Vector2Int.zero;
        private Vector2Int _faceDirection = Vector2Int.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessage();
        }

        private void SubscribeToMessage()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDirectionMessage>(SetDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryDirectionMessage>(QueryDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetFacingDirectionMessage>(SetFacingDirection, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryFacingDirectionMessage>(QueryFacingDirection, _instanceId);
        }

        private void SetDirection(SetDirectionMessage msg)
        {
            if (_direction != msg.Direction)
            {
                _direction = msg.Direction;
                _faceDirection = msg.Direction.ToFaceDirection();
                var updateDirectionMsg = MessageFactory.GenerateUpdateDirectionMsg();
                updateDirectionMsg.Direction = _direction;
                _controller.gameObject.SendMessageTo(updateDirectionMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateDirectionMsg);
            }
        }

        private void QueryDirection(QueryDirectionMessage msg)
        {
            msg.DoAfter.Invoke(_direction);
        }

        private void SetFacingDirection(SetFacingDirectionMessage msg)
        {
            _faceDirection = msg.Direction;
        }

        private void QueryFacingDirection(QueryFacingDirectionMessage msg)
        {
            msg.DoAfter.Invoke(_faceDirection);
        }
    }
}