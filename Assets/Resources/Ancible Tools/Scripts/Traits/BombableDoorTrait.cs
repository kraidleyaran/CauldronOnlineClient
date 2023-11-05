using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Bombable Door Trait", menuName = "Ancible Tools/Traits/Interactable/Bombable Door")]
    public class BombableDoorTrait : Trait
    {
        [SerializeField] private Hitbox.Hitbox _hitbox;

        private HitboxController _hitboxController = null;
        private int _bombableExperience = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _hitboxController = _controller.gameObject.SetupHitbox(_hitbox, CollisionLayerFactory.MonsterHurt);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<TakeDamageMessage>(TakeDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupBombableDoorMessage>(SetupBombableDoor, _instanceId);
        }

        private void TakeDamage(TakeDamageMessage msg)
        {
            if (msg.Tags.Contains(ItemFactory.ExplosiveTag) && msg.Amount > 0 && msg.OwnerId == ObjectManager.PlayerObjectId)
            {
                var objId = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(objId))
                {
                    ClientController.SendToServer(new ClientOpenDoorMessage { DoorId = objId, Tick = TickController.ServerTick});
                }

                _controller.transform.parent.gameObject.SetActive(false);

                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Disabled;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                var gainSkillExperienceMsg = MessageFactory.GenerateGainSkillExperienceMsg();
                gainSkillExperienceMsg.Experience = _bombableExperience;
                _controller.gameObject.SendMessageTo(gainSkillExperienceMsg, ObjectManager.Player);
                MessageFactory.CacheMessage(gainSkillExperienceMsg);
            }
        }

        private void SetupBombableDoor(SetupBombableDoorMessage msg)
        {
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
            _bombableExperience = msg.BombableExperience;
            if (msg.Open)
            {
                _controller.transform.parent.gameObject.SetActive(false);

                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Disabled;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
        }

        public override void Destroy()
        {
            if (_hitboxController)
            {
                Destroy(_hitboxController.gameObject);
            }
            base.Destroy();
        }
    }
}