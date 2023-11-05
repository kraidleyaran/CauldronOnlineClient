using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Movable Helper Trait", menuName = "Ancible Tools/Traits/Network/Network Movable Helper")]
    public class NetworkMovableHelperTrait : Trait
    {
        [SerializeField] private int _maxEvents = 3;

        private GameObject _movable = null;
        private Rigidbody2D _movableRigidBody = null;

        private Rigidbody2D _rigidBody = null;
        
        private List<MovableEvent> _events = new List<MovableEvent>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _rigidBody = _controller.transform.parent.gameObject.GetComponent<Rigidbody2D>();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<MovableEventMessage>(MovableEvent, _instanceId);
        }

        private void MovableEvent(MovableEventMessage msg)
        {
            _events.Add(msg.Event);
            while (_events.Count > _maxEvents)
            {
                _events.RemoveAt(0);
            }
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_events.Count > 0)
            {
                var movableEvent = _events[0];
                switch (movableEvent.Type)
                {
                    case CauldronOnlineCommon.Data.MovableType.Grab:
                        var obj = ObjectManager.GetObjectById(movableEvent.MovableId);
                        if (obj)
                        {
                            _movable = obj;
                            _movableRigidBody = obj.GetComponent<Rigidbody2D>();

                            _movableRigidBody.position = movableEvent.MovablePosition.ToWorldVector();
                            _rigidBody.position = movableEvent.OwnerPosition.ToWorldVector();

                            var direction = (_movableRigidBody.position - _rigidBody.position).normalized.ToFaceDirection();
                            var setFacingDirectionMsg = MessageFactory.GenerateSetFacingDirectionMsg();
                            setFacingDirectionMsg.Direction = direction;
                            _controller.gameObject.SendMessageTo(setFacingDirectionMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setFacingDirectionMsg);

                            var setOwnerMsg = MessageFactory.GenerateSetOwnerMsg();
                            setOwnerMsg.Owner = _controller.transform.parent.gameObject;
                            _controller.gameObject.SendMessageTo(setOwnerMsg, _movable);
                            MessageFactory.CacheMessage(setOwnerMsg);

                            var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                            setUnitAnimationStateMsg.State = UnitAnimationState.Grab;
                            _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                            setUnitStateMsg.State = UnitState.Interaction;
                            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setUnitStateMsg);

                            _events.RemoveAt(0);
                        }
                        break;
                    case CauldronOnlineCommon.Data.MovableType.Move:
                        if (_movable)
                        {
                            var speed = TickController.CalculateFixedMoveSpeed(movableEvent.Speed);
                            var difference = (movableEvent.OwnerPosition.ToWorldVector() - _rigidBody.position);
                            var distance = difference.magnitude;
                            if (distance > speed)
                            {
                                _rigidBody.position += (difference.normalized * speed);
                                _movableRigidBody.position += (difference.normalized * speed);
                            }
                            else
                            {
                                _rigidBody.position = movableEvent.OwnerPosition.ToWorldVector();
                                _movableRigidBody.position = movableEvent.MovablePosition.ToWorldVector();
                                _events.RemoveAt(0);
                            }

                            {
                                var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                                setUnitAnimationStateMsg.State = UnitAnimationState.Push;
                                _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                                MessageFactory.CacheMessage(setUnitAnimationStateMsg);
                            }
                        }
                        break;
                    case CauldronOnlineCommon.Data.MovableType.Release:
                        {
                            if (_movable)
                            {
                                _movableRigidBody.position = movableEvent.MovablePosition.ToWorldVector();
                                var setOwnerMsg = MessageFactory.GenerateSetOwnerMsg();
                                setOwnerMsg.Owner = null;
                                _controller.gameObject.SendMessageTo(setOwnerMsg, _movable);
                                MessageFactory.CacheMessage(setOwnerMsg);

                                _movable = null;
                                _movableRigidBody = null;
                            }

                            _rigidBody.position = movableEvent.OwnerPosition.ToWorldVector();
                            var setUnitAnimationStateMsg = MessageFactory.GenerateSetUnitAnimationStateMsg();
                            setUnitAnimationStateMsg.State = UnitAnimationState.Idle;
                            _controller.gameObject.SendMessageTo(setUnitAnimationStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setUnitAnimationStateMsg);

                            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                            setUnitStateMsg.State = UnitState.Active;
                            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setUnitStateMsg);
                            _events.RemoveAt(0);
                        }
                        break;
                }

            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {

        }
    }
}