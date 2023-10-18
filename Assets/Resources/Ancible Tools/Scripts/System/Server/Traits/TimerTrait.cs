using System;
using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits
{
    [CreateAssetMenu(fileName = "Timer Trait", menuName = "Ancible Tools/Traits/General/Timer")]
    public class TimerTrait : Trait
    {
        [SerializeField] private int _worldTicksPerLoop = 1;
        [SerializeField] private int _loops = 0;
        [SerializeField] private Trait[] _applyOnStart;
        [SerializeField] private Trait[] _applyOnLoop;
        [SerializeField] private bool _refreshTimer = false;

        private TraitController[] _applied = new TraitController[0];

        private Sequence _timerSequence = null;
        private int _currentLoopCount = 0;
        private GameObject _owner = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (_refreshTimer)
            {
                var timerCount = 0;
                var queryTimerMsg = MessageFactory.GenerateQueryTimerMsg();
                queryTimerMsg.DoAfter = () => timerCount++;
                _controller.gameObject.SendMessageWithFilterTo(queryTimerMsg, _controller.transform.parent.gameObject, name);
                MessageFactory.CacheMessage(queryTimerMsg);

                if (timerCount >= MaxStack - 1)
                {
                    _controller.gameObject.SendMessageWithFilterTo(RefreshTimerMessage.INSTANCE, _controller.transform.parent.gameObject, name);

                    var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                    removeTraitFromUnitByControllerMsg.Controller = _controller;
                    _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
                    return;
                }
            }

            _owner = _controller.transform.parent.gameObject;

            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = owner => _owner = owner;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryOwnerMsg);

            if (_applyOnStart.Length > 0)
            {
                var applied = new List<TraitController>();
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyOnStart)
                {
                    if (trait.Instant)
                    {
                        addTraitToUnitMsg.DoAfter = traitController => { };
                    }
                    else
                    {
                        addTraitToUnitMsg.DoAfter = traitController => applied.Add(traitController);
                    }

                    addTraitToUnitMsg.Trait = trait;
                    _owner.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
                _applied = applied.ToArray();
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
            
            StartTimer();
            SubscribeToMessages();
            

        }

        private void StartTimer()
        {
            _timerSequence = DOTween.Sequence().AppendInterval(_worldTicksPerLoop * TickController.WorldTick).OnComplete(LoopFinished);
        }

        private void LoopFinished()
        {
            _timerSequence = null;
            _currentLoopCount++;
            if (_applyOnLoop.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyOnLoop)
                {
                    addTraitToUnitMsg.Trait = trait;
                    _owner.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
            if (_loops < 0 || _currentLoopCount < _loops)
            {
                StartTimer();
            }
            else
            {
                var removeTraitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                if (_applied.Length > 0)
                {
                    foreach (var trait in _applied)
                    {
                        removeTraitByControllerMsg.Controller = trait;
                        _controller.gameObject.SendMessageTo(removeTraitByControllerMsg, _controller.transform.parent.gameObject);
                    }
                }
                removeTraitByControllerMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraitByControllerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraitByControllerMsg);
            }
        }

        public override string GetDescription()
        {
            var description = $"Applies every {_worldTicksPerLoop * TickController.WorldTick:N} seconds for {((_worldTicksPerLoop * _loops + 1) * TickController.WorldTick):N} seconds({_loops + 1}):";
            var traits = _applyOnLoop.GetTraitDescriptions();
            if (traits.Length > 0)
            {
                foreach (var trait in traits)
                {
                    description = $"{description}{Environment.NewLine} > {trait}";
                }
            }

            return description;


        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryTimerMessage>(QueryTimer, name);
            _controller.transform.parent.gameObject.SubscribeWithFilter<RefreshTimerMessage>(RefreshTimer, name);
        }

        private void QueryTimer(QueryTimerMessage msg)
        {
            msg.DoAfter.Invoke();
        }

        private void RefreshTimer(RefreshTimerMessage msg)
        {
            _currentLoopCount = 0;
            if (_timerSequence != null)
            {
                _timerSequence.Restart();
            }
        }

        public override void Destroy()
        {
            if (_timerSequence != null)
            {
                if (_timerSequence.IsActive())
                {
                    _timerSequence.Kill();
                }

                _timerSequence = null;
            }
            _controller.transform.parent.gameObject.UnsubscribeFromAllMessagesWithFilter(name);
            base.Destroy();
        }
    }
}