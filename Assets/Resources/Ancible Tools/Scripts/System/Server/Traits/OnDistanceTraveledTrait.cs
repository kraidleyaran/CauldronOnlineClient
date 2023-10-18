using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "On Distance Traveled Trait", menuName = "Ancible Tools/Traits/General/On Distance Traveled")]
    public class OnDistanceTraveledTrait : Trait
    {
        [SerializeField] private int _distance = 1;
        [SerializeField] private Trait[] _applyOnDistanceTraveled = new Trait[0];

        private float _maxDistance = 0f;
        private float _currentDistance = 0f;

        private Vector2 _position = Vector2.zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _maxDistance = _distance * DataController.Interpolation;

            var queryPositionMsg = MessageFactory.GenerateQueryPositionMsg();
            queryPositionMsg.DoAfter = SetPosition;
            _controller.gameObject.SendMessageTo(queryPositionMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryPositionMsg);
            SubscribeToMessages();
        }

        private void SetPosition(Vector2 position)
        {
            _position = position;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdatePositionMessage>(UpdatePosition, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ResetMaxDistanceCheckMessage>(ResetMaxDistanceCheck, _instanceId);
        }

        private void UpdatePosition(UpdatePositionMessage msg)
        {
            var distance = (msg.Position - _position).magnitude;
            _currentDistance += distance;
            _position = msg.Position;

            if (_currentDistance >= _maxDistance)
            {
                if (_applyOnDistanceTraveled.Length > 0)
                {
                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in _applyOnDistanceTraveled)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);
                }

                var removeTraintFromUnitByController = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removeTraintFromUnitByController.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraintFromUnitByController, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraintFromUnitByController);
            }
        }

        private void ResetMaxDistanceCheck(ResetMaxDistanceCheckMessage msg)
        {
            _currentDistance = 0f;
        }
    }
}