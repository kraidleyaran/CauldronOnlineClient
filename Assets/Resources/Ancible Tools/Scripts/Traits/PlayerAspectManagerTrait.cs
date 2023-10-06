using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Aspects;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{

    [CreateAssetMenu(fileName = "Player Aspect Manager Trait", menuName = "Ancible Tools/Traits/Player/Player Aspect Manager")]
    public class PlayerAspectManagerTrait : Trait
    {
        [SerializeField] private WorldAspect[] _startingAspects = new WorldAspect[0];
        [SerializeField] private float _experienceRequirementMultiplierPerLevel = .15f;
        [SerializeField] private int _baseRequiredExperience = 100;

        private Dictionary<WorldAspect, WorldAspectInstance> _aspects = new Dictionary<WorldAspect, WorldAspectInstance>();

        private int _availablePoints = 0;
        private int _level = 0;
        private int _experience = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            foreach (var aspect in _startingAspects)
            {
                if (!_aspects.ContainsKey(aspect))
                {
                    _aspects.Add(aspect, new WorldAspectInstance(aspect));
                }
            }
            SubscribeToMessages();
        }

        private int CalculateLevelExperience(int level)
        {
            return level > 1 ? Mathf.RoundToInt(_baseRequiredExperience + _baseRequiredExperience * (level * _experienceRequirementMultiplierPerLevel)) : _baseRequiredExperience;
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<ClientExperienceMessage>(ClientExperience);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyAspectRanksMessage>(ApplyAspectRanks, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAspectsMessage>(QueryAspects, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddAspectMessage>(AddAspect, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryExperienceMessage>(QueryExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetLoadedAspectsMessage>(SetLoadedAspects, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetExperienceMessage>(SetExperience, _instanceId);
        }

        private void ApplyAspectRanks(ApplyAspectRanksMessage msg)
        {
            if (_aspects.TryGetValue(msg.Aspect, out var instance))
            {
                instance.Apply(msg.Ranks, _controller.transform.parent.gameObject, msg.Bonus);
                _availablePoints = Mathf.Max(0, _availablePoints - msg.Ranks);
            }

            if (msg.Update)
            {
                _controller.gameObject.SendMessage(PlayerAspectsUpdatedMessage.INSTANCE);
            }
        }

        private void QueryAspects(QueryAspectsMessage msg)
        {
            msg.DoAfter.Invoke(_aspects.Values.ToArray(), _availablePoints);
        }

        private void AddAspect(AddAspectMessage msg)
        {
            if (!_aspects.ContainsKey(msg.Aspect))
            {
                _aspects.Add(msg.Aspect, new WorldAspectInstance(msg.Aspect));
            }
            _controller.gameObject.SendMessage(PlayerAspectsUpdatedMessage.INSTANCE);
        }

        private void ClientExperience(ClientExperienceMessage msg)
        {
            _experience += msg.Amount;
            var required = CalculateLevelExperience(_level + 1);
            var leveled = false;
            while (required < _experience)
            {
                leveled = true;
                _experience -= required;
                _level++;
                _availablePoints++;
                required = CalculateLevelExperience(_level + 1);
            }

            if (leveled)
            {
                _controller.gameObject.SendMessage(PlayerAspectsUpdatedMessage.INSTANCE);
            }
            _controller.gameObject.SendMessage(PlayerExperienceUpdatedMessage.INSTANCE);
        }

        private void QueryExperience(QueryExperienceMessage msg)
        {
            msg.DoAfter.Invoke(_level, _experience, CalculateLevelExperience(_level + 1));
        }

        private void SetLoadedAspects(SetLoadedAspectsMessage msg)
        {
            _availablePoints = msg.AvailablePoints;

            foreach (var aspectData in msg.Aspects)
            {
                var aspect = AspectFactory.GetAspectByName(aspectData.Name);
                if (aspect)
                {
                    if (!_aspects.ContainsKey(aspect))
                    {
                        _aspects.Add(aspect, new WorldAspectInstance(aspect));
                    }

                    var totalAdd = aspectData.Rank - _aspects[aspect].Rank;
                    aspect.ApplyRank(totalAdd, _controller.transform.parent.gameObject);
                }
            }
            _controller.gameObject.SendMessage(PlayerAspectsUpdatedMessage.INSTANCE);
        }

        private void SetExperience(SetExperienceMessage msg)
        {
            _experience = msg.Experience;
            _level = msg.Level;
            _controller.gameObject.SendMessage(PlayerExperienceUpdatedMessage.INSTANCE);
        }

    }
}