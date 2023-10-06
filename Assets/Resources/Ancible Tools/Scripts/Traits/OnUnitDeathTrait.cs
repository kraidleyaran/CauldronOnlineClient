using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using DG.Tweening;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "On Unit Death Trait", menuName = "Ancible Tools/Traits/Combat/On Unit Death")]
    public class OnUnitDeathTrait : Trait
    {
        [SerializeField] private Trait[] _applyOnDeath;
        [SerializeField] private int _deathTicks = 120;


        private Sequence _deathTimer = null;
        private UnitState _unitState = UnitState.Active;

        private bool _destroyOnDeath = false;
        private bool _reportDeath = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void DeathTimerCompleted()
        {
            if (_destroyOnDeath)
            {
                ObjectManager.DestroyNetworkObject(_controller.transform.parent.gameObject);
            }
            else if (_unitState == UnitState.Dead)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Disabled;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDeathMessage>(UnitDeath, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitStateMessage>(UpdateUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<IsMonsterMessage>(IsMonster, _instanceId);
        }

        private void UnitDeath(UnitDeathMessage msg)
        {
            if (_unitState != UnitState.Dead)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Dead;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                if (_applyOnDeath.Length > 0)
                {
                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in _applyOnDeath)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);
                }

                if (_reportDeath)
                {
                    var id = ObjectManager.GetId(_controller.transform.parent.gameObject);
                    if (!string.IsNullOrEmpty(id))
                    {
                        ClientController.SendToServer(new ClientDeathMessage { Target = id });
                    }
                }
            }

            if (_deathTimer == null)
            {
                _deathTimer = DOTween.Sequence().AppendInterval(_deathTicks * TickController.TickRate).OnComplete(DeathTimerCompleted);
            }

        }

        private void UpdateUnitState(UpdateUnitStateMessage msg)
        {
            _unitState = msg.State;
            if (_unitState != UnitState.Dead && _unitState != UnitState.Disabled && _deathTimer != null)
            {
                if (_deathTimer.IsActive())
                {
                    _deathTimer.Kill();
                }

                _deathTimer = null;
            }
        }

        private void IsMonster(IsMonsterMessage msg)
        {
            _destroyOnDeath = true;
            _reportDeath = true;
        }
    }
}