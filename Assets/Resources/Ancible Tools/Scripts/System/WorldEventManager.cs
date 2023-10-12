using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.WorldEvents;
using ConcurrentMessageBus;
using UnityEngine;
using Object = System.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class WorldEventManager : MonoBehaviour
    {
        private static WorldEventManager _instance = null;

        private bool _active = false;

        private ApplyMovementEventMessage _applyMovementEventMsg = new ApplyMovementEventMessage();
        private ApplyKnockbackEventMessage _applyKnockbackEventMsg = new ApplyKnockbackEventMessage();
        private UseAbilityMessage _useAbilitMsg = new UseAbilityMessage();
        private SetFacingDirectionMessage _setFacingDirectionMsg = new SetFacingDirectionMessage();
        private SetWorldPositionMessage _setWorldPositionMsg = new SetWorldPositionMessage();
        private SetCurrentAggroTargetMessage _setCurrentAggroTargetMsg = new SetCurrentAggroTargetMessage();
        private SetPathMessage _setPathMsg = new SetPathMessage();
        private SetDoorStateMessage _setDoorStateMsg = new SetDoorStateMessage();
        private SetSignalMessage _setSignalMsg = new SetSignalMessage();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            SubscribeToMessags();
        }

        public static void SetActiveState(bool active)
        {
            _instance._active = active;
        }
        
        public static void SetRunningState(bool running)
        {
            //if (_instance._running && !running)
            //{
            //    _instance._running = false;
            //}
            //else if (running && !_instance._running)
            //{
            //    _instance._running = true;
            //}
        }

        private void SpawnLoot(SpawnLootEvent spawnEvent, Vector2 position)
        {
            var spawnCount = spawnEvent.Drops.Roll(true);
            if (spawnCount > 0)
            {
                for (var i = 0; i < spawnCount; i++)
                {
                    var itemRoll = Random.Range(0f, 1f);
                    var availbleItems = spawnEvent.LootTable.LootRolls.Where(l => l.ChanceToDrop >= itemRoll).ToArray();
                    if (availbleItems.Length > 0)
                    {
                        var loot = availbleItems.Length > 1 ? availbleItems[Random.Range(0, availbleItems.Length)] : availbleItems[0];
                        var stack = loot.Stack.Roll(true);
                        var item = ItemFactory.GetItemByName(loot.Item);
                        if (stack > 0 && item)
                        {
                            if (loot.SpawnEachStack && stack > 1)
                            {
                                for (var s = 0; s < stack; s++)
                                {
                                    var controller = Instantiate(ItemFactory.ItemLoot, position, Quaternion.identity);
                                    controller.Setup(item, 1, StaticMethods.RandomDirection(), false);
                                    ObjectManager.RegisterObject(controller.gameObject);
                                }
                            }
                            else
                            {
                                var controller = Instantiate(ItemFactory.ItemLoot, position, Quaternion.identity);
                                controller.Setup(item, stack, StaticMethods.RandomDirection(), true);
                                ObjectManager.RegisterObject(controller.gameObject);
                            }

                        }
                    }
                }
            }

            


        }


        private void SubscribeToMessags()
        {
            gameObject.Subscribe<ClientWorldEventsUpdateMessage>(ClientWorldEventsUpdate);
        }

        private void ClientWorldEventsUpdate(ClientWorldEventsUpdateMessage msg)
        {
            if (_active)
            {
                var worldEvents = new List<WorldEvent>();
                foreach (var data in msg.Events)
                {
                    var worldEvent = data.GenerateEventFromData();
                    worldEvents.Add(worldEvent);
                }
                worldEvents = worldEvents.OrderBy(e => e.Tick).ThenBy(e => e.Order).ToList();
                foreach (var worldEvent in worldEvents)
                {
                    switch (worldEvent.EventId)
                    {
                        case ObjectCreatedEvent.ID:
                            if (worldEvent is ObjectCreatedEvent objectCreated)
                            {
                                ObjectManager.GenerateNetworkObject(objectCreated.Data, objectCreated.ShowAppearance);
                            }
                            break;
                        case MovementEvent.ID:
                            if (worldEvent is MovementEvent movement)
                            {
                                var obj = ObjectManager.GetObjectById(movement.Id);
                                if (obj)
                                {
                                    _applyMovementEventMsg.Event = movement;
                                    gameObject.SendMessageTo(_applyMovementEventMsg, obj);
                                }
                            }
                            break;
                        case DamageEvent.ID:
                            if (worldEvent is DamageEvent damage && damage.OwnerId != ObjectManager.PlayerObjectId)
                            {
                                var obj = ObjectManager.GetObjectById(damage.TargetId);
                                if (obj)
                                {
                                    var takeDamageMsg = MessageFactory.GenerateTakeDamageMsg();
                                    takeDamageMsg.Amount = damage.Amount;
                                    takeDamageMsg.Event = true;
                                    gameObject.SendMessageTo(takeDamageMsg, obj);
                                    MessageFactory.CacheMessage(takeDamageMsg);
                                }
                            }
                            break;
                        case KnockbackEvent.ID:
                            if (worldEvent is KnockbackEvent knockback && knockback.OwnerId != ObjectManager.PlayerObjectId)
                            {
                                var obj = ObjectManager.GetObjectById(knockback.TargetId);
                                if (obj)
                                {
                                    _applyKnockbackEventMsg.Position = knockback.EndPosition;
                                    _applyKnockbackEventMsg.Time = knockback.Time;
                                    gameObject.SendMessageTo(_applyKnockbackEventMsg, obj);
                                }
                            }
                            break;
                        case AbilityEvent.ID:
                            if (worldEvent is AbilityEvent ability)
                            {
                                var obj = ObjectManager.GetObjectById(ability.OwnerId);
                                if (obj && !string.IsNullOrEmpty(ability.Ability))
                                {
                                    var worldAbility = AbilityFactory.GetAbilityByName(ability.Ability);
                                    if (worldAbility)
                                    {
                                        _setWorldPositionMsg.Position = ability.Position;
                                        gameObject.SendMessageTo(_setWorldPositionMsg, obj);

                                        _setFacingDirectionMsg.Direction = ability.Direction.ToVector();
                                        gameObject.SendMessageTo(_setFacingDirectionMsg, obj);

                                        if (!string.IsNullOrEmpty(ability.TargetId))
                                        {
                                            var target = ObjectManager.GetObjectById(ability.TargetId);
                                            if (target)
                                            {
                                                _setCurrentAggroTargetMsg.Target = target;
                                                gameObject.SendMessageTo(_setCurrentAggroTargetMsg, obj);
                                            }
                                        }

                                        _useAbilitMsg.Ability = worldAbility;
                                        _useAbilitMsg.Ids = ability.Ids;
                                        gameObject.SendMessageTo(_useAbilitMsg, obj);
                                    }
                                    
                                }
                                
                            }
                            break;
                        case ObjectDeathEvent.ID:
                            if (worldEvent is ObjectDeathEvent death && death.OwnerId != ObjectManager.PlayerObjectId)
                            {
                                var obj = ObjectManager.GetObjectById(death.Id);
                                if (obj)
                                {
                                    gameObject.SendMessageTo(UnitDeathMessage.INSTANCE, obj);
                                }
                            }
                            break;
                        case DestroyObjectEvent.ID:
                            if (worldEvent is DestroyObjectEvent destroy && destroy.OwnerId != ObjectManager.PlayerObjectId)
                            {
                                var obj = ObjectManager.GetObjectById(destroy.ObjectId);
                                if (obj)
                                {
                                    var unitState = UnitState.Active;

                                    var queryUnitStateMsg = MessageFactory.GenerateQueryUnitStateMsg();
                                    queryUnitStateMsg.DoAfter = (state) => unitState = state;
                                    gameObject.SendMessageTo(queryUnitStateMsg, obj);
                                    MessageFactory.CacheMessage(queryUnitStateMsg);

                                    if (unitState != UnitState.Dead)
                                    {
                                        ObjectManager.DestroyNetworkObject(obj);
                                    }
                                }
                            }
                            break;
                        case SpawnLootEvent.ID:
                            if (worldEvent is SpawnLootEvent spawnLoot)
                            {
                                var obj = ObjectManager.GetObjectById(spawnLoot.OwnerId);
                                var pos = spawnLoot.Position.ToWorldVector();
                                if (obj)
                                {
                                    pos = obj.transform.position.ToVector2().ToWorldPosition().ToWorldVector();
                                }
                                SpawnLoot(spawnLoot, pos);
                            }
                            break;
                        case RespawnEvent.ID:
                            if (worldEvent is RespawnEvent respawn)
                            {
                                var obj = ObjectManager.GetObjectById(respawn.OwnerId);
                                if (obj)
                                {
                                    _setWorldPositionMsg.Position = respawn.Position;
                                    gameObject.SendMessageTo(_setWorldPositionMsg, obj);

                                    gameObject.SendMessageTo(FullHealMessage.INSTANCE, obj);

                                    var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                                    setUnitStateMsg.State = UnitState.Active;
                                    gameObject.SendMessageTo(setUnitStateMsg, obj);
                                    MessageFactory.CacheMessage(setUnitStateMsg);

                                    obj.gameObject.SetActive(true);
                                }
                            }
                            break;
                        case UpdatePathEvent.ID:
                            if (worldEvent is UpdatePathEvent updatePath)
                            {
                                var obj = ObjectManager.GetObjectById(updatePath.OwnerId);
                                if (obj)
                                {
                                    _setPathMsg.Path = updatePath.Path;
                                    _setPathMsg.MoveSpeed = updatePath.Speed;
                                    gameObject.SendMessageTo(_setPathMsg, obj);
                                }
                            }
                            break;
                        case HealEvent.ID:
                            if (worldEvent is HealEvent heal)
                            {
                                var obj = ObjectManager.GetObjectById(heal.TargetId);
                                if (obj)
                                {
                                    var healMsg = MessageFactory.GenerateHealMsg();
                                    healMsg.Amount = heal.Amount;
                                    healMsg.IsEvent = true;
                                    healMsg.OwnerId = heal.OwnerId;
                                    gameObject.SendMessageTo(healMsg, obj);
                                    MessageFactory.CacheMessage(healMsg);
                                }
                            }

                            break;
                        case DoorEvent.ID:
                            if (worldEvent is DoorEvent door && door.OwnerId != ObjectManager.PlayerObjectId)
                            {
                                var obj = ObjectManager.GetObjectById(door.TargetId);
                                if (obj)
                                {
                                    _setDoorStateMsg.Open = door.Open;
                                    obj.SendMessageTo(_setDoorStateMsg, obj);
                                }
                            }
                            break;
                        case SwitchSignalEvent.ID:
                            if (worldEvent is SwitchSignalEvent signalEvent)
                            {
                                var obj = ObjectManager.GetObjectById(signalEvent.TargetId);
                                if (obj)
                                {
                                    _setSignalMsg.Signal = signalEvent.Signal;
                                    obj.SendMessageTo(_setSignalMsg, obj);
                                }
                            }
                            break;
                        case ChestOpenEvent.ID:
                            if (worldEvent is ChestOpenEvent chest)
                            {
                                var obj = ObjectManager.GetObjectById(chest.TargetId);
                                if (obj)
                                {
                                    obj.SendMessageTo(OpenChestMessage.INSTANCE, obj);
                                }
                            }
                            break;
                        case ChestRefillEvent.ID:
                            if (worldEvent is ChestRefillEvent refill)
                            {
                                var obj = ObjectManager.GetObjectById(refill.TargetId);
                                if (obj)
                                {
                                    obj.SendMessageTo(CloseChestMessage.INSTANCE, obj);
                                }
                            }
                            break;
                        case KeyItemLootEvent.ID:
                            if (worldEvent is KeyItemLootEvent keyItem)
                            {
                                var item = ItemFactory.GetItemByName(keyItem.Item);
                                if (item)
                                {
                                    var pos = keyItem.Position.ToWorldVector();
                                    var obj = ObjectManager.GetObjectById(keyItem.TargetId);
                                    if (obj)
                                    {
                                        pos = obj.transform.position.ToVector2();
                                    }

                                    var controller = Instantiate(ItemFactory.KeyItemLoot, pos, Quaternion.identity);
                                    ObjectManager.RegisterObject(controller.gameObject);
                                    controller.Setup(item);
                                }
                            }
                            break;
                        case TeleportEvent.ID:
                            if (worldEvent is TeleportEvent teleport)
                            {
                                var obj = ObjectManager.GetObjectById(teleport.ObjectId);
                                if (obj)
                                {
                                    var setWorldPositionMsg = MessageFactory.GenerateSetWorldPositionMsg();
                                    setWorldPositionMsg.Position = teleport.Position;
                                    obj.SendMessageTo(setWorldPositionMsg, obj);
                                    MessageFactory.CacheMessage(setWorldPositionMsg);
                                }
                            }
                            break;
                    }
                }
            }
            
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}