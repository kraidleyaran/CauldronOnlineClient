using System;
using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using MessageBusLib;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public static class MessageFactory
    {
        private static List<AddTraitToUnitMessage> _addTraitToUnitCache = new List<AddTraitToUnitMessage>();
        private static List<RemoveTraitFromUnitMessage> _removeTraitFromUnitCache = new List<RemoveTraitFromUnitMessage>();
        private static List<RemoveTraitFromUnitByControllerMessage> _removeTraitFromUnitByControllerCache = new List<RemoveTraitFromUnitByControllerMessage>();
        private static List<TraitCheckMessage> _traitCheckCache = new List<TraitCheckMessage>();
        private static List<HitboxCheckMessage> _hitboxCheckCache = new List<HitboxCheckMessage>();
        private static List<EnterCollisionWithObjectMessage> _enterCollisiongWithObjectCache = new List<EnterCollisionWithObjectMessage>();
        private static List<ExitCollisionWithObjectMessage> _exitCollisionWithObjectCache = new List<ExitCollisionWithObjectMessage>();
        private static List<RegisterCollisionMessage> _registerCollisionCache = new List<RegisterCollisionMessage>();
        private static List<UnregisterCollisionMessage> _unregisterCollisionCache = new List<UnregisterCollisionMessage>();
        private static List<SetPositionMessage> _setPositionCache = new List<SetPositionMessage>();
        private static List<UpdatePositionMessage> _updatePositionCache = new List<UpdatePositionMessage>();
        private static List<QueryPositionMessage> _queryPositionCache = new List<QueryPositionMessage>();
        private static List<SetDirectionMessage> _setDirectionCache = new List<SetDirectionMessage>();
        private static List<UpdateDirectionMessage> _updateDirectionCache = new List<UpdateDirectionMessage>();
        private static List<QueryDirectionMessage> _queryDirectionCache = new List<QueryDirectionMessage>();
        private static List<SetUnitAnimationStateMessage> _setUnitAnimationStateCache = new List<SetUnitAnimationStateMessage>();
        private static List<TakeDamageMessage> _takeDamageCache = new List<TakeDamageMessage>();
        private static List<SetOwnerMessage> _setOwnerCache = new List<SetOwnerMessage>();
        private static List<UpdateOwnerMessage> _updateOwnerCache = new List<UpdateOwnerMessage>();
        private static List<QueryOwnerMessage> _queryOwnerCache = new List<QueryOwnerMessage>();
        private static List<QueryCombatStatsMessage> _queryCombatStatsCache = new List<QueryCombatStatsMessage>();
        private static List<ApplyFlashColorFxMessage> _applyFlashColorFxCache = new List<ApplyFlashColorFxMessage>();
        private static List<UpdateKnockbackStateMessage> _updateKnockbackStateCache = new List<UpdateKnockbackStateMessage>();
        private static List<ApplyKnockbackMessage> _applyKnockbackCache = new List<ApplyKnockbackMessage>();
        private static List<SetUnitStateMessage> _setUnitStateCache = new List<SetUnitStateMessage>();
        private static List<UpdateUnitStateMessage> _updateUnitStateCache = new List<UpdateUnitStateMessage>();
        private static List<QueryUnitStateMessage> _queryUnitStateCache = new List<QueryUnitStateMessage>();
        private static List<WalledCheckMessage> _walledCheckCache = new List<WalledCheckMessage>();
        private static List<UpdateAbilityStateMessage> _updateAbilityStateCache = new List<UpdateAbilityStateMessage>();
        private static List<UpdateFacingDirectionMessage> _updateFacingDirectionCache = new List<UpdateFacingDirectionMessage>();
        private static List<QueryFacingDirectionMessage> _queryFacingDirectionCache = new List<QueryFacingDirectionMessage>();
        private static List<SetupProjectileMessage> _setupProjectileCache = new List<SetupProjectileMessage>();
        private static List<UpdateWorldPositionMessage> _updateWorldPositionCache = new List<UpdateWorldPositionMessage>();
        private static List<UseAbilityMessage> _useAbilityCache = new List<UseAbilityMessage>();
        private static List<AddItemMessage> _addItemCache = new List<AddItemMessage>();
        private static List<RemoveItemMessage> _removeItemCache = new List<RemoveItemMessage>();
        private static List<QueryInventoryMessage> _queryInventoryCache = new List<QueryInventoryMessage>();
        private static List<AddGoldMessage> _addGoldCache = new List<AddGoldMessage>();
        private static List<RemoveGoldMessage> _removeGoldCache = new List<RemoveGoldMessage>();
        private static List<QueryGoldMessage> _queryGoldCache = new List<QueryGoldMessage>();
        private static List<QueryLoadoutMessage> _queryLoadoutCache = new List<QueryLoadoutMessage>();
        private static List<QueryItemsMessage> _queryItemsCache = new List<QueryItemsMessage>();
        private static List<HasItemsQueryMessage> _hasItemsCache = new List<HasItemsQueryMessage>();
        private static List<EquipItemToLoadoutSlotMessage> _equipItemToLoadoutSlotCache = new List<EquipItemToLoadoutSlotMessage>();
        private static List<EquipAbilityToLoadoutSlotMessage> _equipAbilityToLoadoutSlotCache = new List<EquipAbilityToLoadoutSlotMessage>();
        private static List<QueryAbilitiesMessage> _queryAbilitiesCache = new List<QueryAbilitiesMessage>();
        private static List<UnequipLoadoutSlotMessage> _unequipLoadoutSlotCache = new List<UnequipLoadoutSlotMessage>();
        private static List<QueryTargetMessage> _queryTargetCache = new List<QueryTargetMessage>();
        private static List<SetPathMessage> _setPathCache = new List<SetPathMessage>();
        private static List<QueryWorldPositionMessage> _queryWorldPositionCache = new List<QueryWorldPositionMessage>();
        private static List<ApplyAspectRanksMessage> _applyAspectRanksCache = new List<ApplyAspectRanksMessage>();
        private static List<QueryAspectsMessage> _queryAspectsCache = new List<QueryAspectsMessage>();
        private static List<AddAspectMessage> _addAspectCache = new List<AddAspectMessage>();
        private static List<QueryExperienceMessage> _queryExperienceCache = new List<QueryExperienceMessage>();
        private static List<ApplyCombatStatsMessage> _applyCombatStatsCache = new List<ApplyCombatStatsMessage>();
        private static List<EquipArmorItemMessage> _equipArmorItemCache = new List<EquipArmorItemMessage>();
        private static List<UnequipArmorItemFromSlotMessage> _unequipArmorItemFromSlotCache = new List<UnequipArmorItemFromSlotMessage>();
        private static List<QueryArmorEquipmentMessage> _queryArmoyEquipmentCache = new List<QueryArmorEquipmentMessage>();
        private static List<QueryAbilityCooldownMessage> _queryAbilityCooldownCache = new List<QueryAbilityCooldownMessage>();
        private static List<QueryShopMessage> _queryShopCache = new List<QueryShopMessage>();
        private static List<SetInteractionMessage> _setInteractionCache = new List<SetInteractionMessage>();
        private static List<RemoveInteractionMessage> _removeInteractionCache = new List<RemoveInteractionMessage>();
        private static List<SetWorldPositionMessage> _setworldPositionCache = new List<SetWorldPositionMessage>();
        private static List<HealMessage> _healCache = new List<HealMessage>();
        private static List<SetDialogueMessage> _setDialogueCache = new List<SetDialogueMessage>();
        private static List<QueryDamageBonusMessage> _queryDamageBonusCache = new List<QueryDamageBonusMessage>();
        private static List<ApplySecondaryStatsMessage> _applySecondaryStatsCache = new List<ApplySecondaryStatsMessage>();
        private static List<QueryAvailableResourceUsesMessage> _queryAvailableResourceUsesCache = new List<QueryAvailableResourceUsesMessage>();
        
        public static AddTraitToUnitMessage GenerateAddTraitToUnitMsg()
        {
            if (_addTraitToUnitCache.Count > 0)
            {
                var message = _addTraitToUnitCache[0];
                _addTraitToUnitCache.Remove(message);
                return message;
            }

            return new AddTraitToUnitMessage();
        }

        public static RemoveTraitFromUnitMessage GenerateRemoveTraitFromUnitMsg()
        {
            if (_removeTraitFromUnitCache.Count > 0)
            {
                var message = _removeTraitFromUnitCache[0];
                _removeTraitFromUnitCache.Remove(message);
                return message;
            }

            return new RemoveTraitFromUnitMessage();
        }

        public static RemoveTraitFromUnitByControllerMessage GenerateRemoveTraitFromUnitByControllerMsg()
        {
            if (_removeTraitFromUnitByControllerCache.Count > 0)
            {
                var message = _removeTraitFromUnitByControllerCache[0];
                _removeTraitFromUnitByControllerCache.Remove(message);
                return message;
            }

            return new RemoveTraitFromUnitByControllerMessage();
        }

        public static HitboxCheckMessage GenerateHitboxCheckMsg()
        {
            if (_hitboxCheckCache.Count > 0)
            {
                var message = _hitboxCheckCache[0];
                _hitboxCheckCache.Remove(message);
                return message;
            }

            return new HitboxCheckMessage();
        }

        public static EnterCollisionWithObjectMessage GenerateEnterCollisionWithObjectMsg()
        {
            if (_enterCollisiongWithObjectCache.Count > 0)
            {
                var message = _enterCollisiongWithObjectCache[0];
                _enterCollisiongWithObjectCache.Remove(message);
                return message;
            }

            return new EnterCollisionWithObjectMessage();
        }

        public static ExitCollisionWithObjectMessage GenerateExitCollisionWithObjectMsg()
        {
            if (_exitCollisionWithObjectCache.Count > 0)
            {
                var message = _exitCollisionWithObjectCache[0];
                _exitCollisionWithObjectCache.Remove(message);
                return message;
            }

            return new ExitCollisionWithObjectMessage();
        }

        public static TraitCheckMessage GenerateTraitCheckMsg()
        {
            if (_traitCheckCache.Count > 0)
            {
                var message = _traitCheckCache[0];
                _traitCheckCache.Remove(message);
                return message;
            }

            return new TraitCheckMessage();
        }

        public static RegisterCollisionMessage GenerateRegisterCollisionMsg()
        {
            if (_registerCollisionCache.Count > 0)
            {
                var message = _registerCollisionCache[0];
                _registerCollisionCache.Remove(message);
                return message;
            }

            return new RegisterCollisionMessage();
        }

        public static UnregisterCollisionMessage GenerateUnregisterCollisionMsg()
        {
            if (_unregisterCollisionCache.Count > 0)
            {
                var message = _unregisterCollisionCache[0];
                _unregisterCollisionCache.Remove(message);
                return message;
            }

            return new UnregisterCollisionMessage();
        }

        public static SetPositionMessage GenerateSetPositionMsg()
        {
            if (_setPositionCache.Count > 0)
            {
                var message = _setPositionCache[0];
                _setPositionCache.Remove(message);
                return message;
            }

            return new SetPositionMessage();
        }


        public static UpdatePositionMessage GenerateUpdatePositionMsg()
        {
            if (_updatePositionCache.Count > 0)
            {
                var message = _updatePositionCache[0];
                _updatePositionCache.Remove(message);
                return message;
            }

            return new UpdatePositionMessage();
        }

        public static QueryPositionMessage GenerateQueryPositionMsg()
        {
            if (_queryPositionCache.Count > 0)
            {
                var message = _queryPositionCache[0];
                _queryPositionCache.Remove(message);
                return message;
            }

            return new QueryPositionMessage();
        }

        public static SetDirectionMessage GenerateSetDirectionMsg()
        {
            if (_setDirectionCache.Count > 0)
            {
                var message = _setDirectionCache[0];
                _setDirectionCache.Remove(message);
                return message;
            }

            return new SetDirectionMessage();
        }

        public static UpdateDirectionMessage GenerateUpdateDirectionMsg()
        {
            if (_updateDirectionCache.Count > 0)
            {
                var message = _updateDirectionCache[0];
                _updateDirectionCache.Remove(message);
                return message;
            }

            return new UpdateDirectionMessage();
        }

        public static QueryDirectionMessage GenerateQueryDirectionMsg()
        {
            if (_queryDirectionCache.Count > 0)
            {
                var message = _queryDirectionCache[0];
                _queryDirectionCache.Remove(message);
                return message;
            }

            return new QueryDirectionMessage();
        }

        public static SetUnitAnimationStateMessage GenerateSetUnitAnimationStateMsg()
        {
            if (_setUnitAnimationStateCache.Count > 0)
            {
                var message = _setUnitAnimationStateCache[0];
                _setUnitAnimationStateCache.Remove(message);
                return message;
            }

            return new SetUnitAnimationStateMessage();
        }

        public static TakeDamageMessage GenerateTakeDamageMsg()
        {
            if (_takeDamageCache.Count > 0)
            {
                var message = _takeDamageCache[0];
                _takeDamageCache.Remove(message);
                return message;
            }

            return new TakeDamageMessage();
        }

        public static SetOwnerMessage GenerateSetOwnerMsg()
        {
            if (_setOwnerCache.Count > 0)
            {
                var message = _setOwnerCache[0];
                _setOwnerCache.Remove(message);
                return message;
            }

            return new SetOwnerMessage();
        }

        public static UpdateOwnerMessage GenerateUpdateOwnerMsg()
        {
            if (_updateOwnerCache.Count > 0)
            {
                var message = _updateOwnerCache[0];
                _updateOwnerCache.Remove(message);
                return message;
            }

            return new UpdateOwnerMessage();
        }

        public static QueryOwnerMessage GenerateQueryOwnerMsg()
        {
            if (_queryOwnerCache.Count > 0)
            {
                var message = _queryOwnerCache[0];
                _queryOwnerCache.Remove(message);
                return message;
            }

            return new QueryOwnerMessage();
        }

        public static QueryCombatStatsMessage GenerateQueryCombatStatsMsg()
        {
            if (_queryCombatStatsCache.Count > 0)
            {
                var message = _queryCombatStatsCache[0];
                _queryCombatStatsCache.Remove(message);
                return message;
            }

            return new QueryCombatStatsMessage();
        }

        public static ApplyFlashColorFxMessage GenerateApplyFlashColorFxMsg()
        {
            if (_applyFlashColorFxCache.Count > 0)
            {
                var message = _applyFlashColorFxCache[0];
                _applyFlashColorFxCache.Remove(message);
                return message;
            }

            return new ApplyFlashColorFxMessage();
        }

        public static ApplyKnockbackMessage GenerateApplyKnockbackMsg()
        {
            if (_applyKnockbackCache.Count > 0)
            {
                var message = _applyKnockbackCache[0];
                _applyKnockbackCache.Remove(message);
                return message;
            }

            return new ApplyKnockbackMessage();
        }

        public static UpdateKnockbackStateMessage GenerateUpdateKnockbackStateMsg()
        {
            if (_updateKnockbackStateCache.Count > 0)
            {
                var message = _updateKnockbackStateCache[0];
                _updateKnockbackStateCache.Remove(message);
                return message;
            }

            return new UpdateKnockbackStateMessage();
        }

        public static SetUnitStateMessage GenerateSetUnitStateMsg()
        {
            if (_setUnitStateCache.Count > 0)
            {
                var message = _setUnitStateCache[0];
                _setUnitStateCache.Remove(message);
                return message;
            }

            return new SetUnitStateMessage();
        }

        public static UpdateUnitStateMessage GenerateUpdateUnitStateMsg()
        {
            if (_updateUnitStateCache.Count > 0)
            {
                var message = _updateUnitStateCache[0];
                _updateUnitStateCache.Remove(message);
                return message;
            }

            return new UpdateUnitStateMessage();
        }

        public static QueryUnitStateMessage GenerateQueryUnitStateMsg()
        {
            if (_queryUnitStateCache.Count > 0)
            {
                var message = _queryUnitStateCache[0];
                _queryUnitStateCache.Remove(message);
                return message;
            }

            return new QueryUnitStateMessage();
        }

        public static WalledCheckMessage GenerateWalledCheckMsg()
        {
            if (_walledCheckCache.Count > 0)
            {
                var message = _walledCheckCache[0];
                _walledCheckCache.Remove(message);
                return message;
            }

            return new WalledCheckMessage();
        }

        public static UpdateAbilityStateMessage GenerateUpdateAbilityStateMsg()
        {
            if (_updateAbilityStateCache.Count > 0)
            {
                var message = _updateAbilityStateCache[0];
                _updateAbilityStateCache.Remove(message);
                return message;
            }

            return new UpdateAbilityStateMessage();
        }

        public static UpdateFacingDirectionMessage GenerateUpdateFacingDirectionMsg()
        {
            if (_updateFacingDirectionCache.Count > 0)
            {
                var message = _updateFacingDirectionCache[0];
                _updateFacingDirectionCache.Remove(message);
                return message;
            }

            return new UpdateFacingDirectionMessage();
        }

        public static QueryFacingDirectionMessage GenerateQueryFacingDirectionMsg()
        {
            if (_queryFacingDirectionCache.Count > 0)
            {
                var message = _queryFacingDirectionCache[0];
                _queryFacingDirectionCache.Remove(message);
                return message;
            }

            return new QueryFacingDirectionMessage();
        }

        public static SetupProjectileMessage GenerateSetupProjectileMsg()
        {
            if (_setupProjectileCache.Count > 0)
            {
                var message = _setupProjectileCache[0];
                _setupProjectileCache.Remove(message);
                return message;
            }

            return new SetupProjectileMessage();
        }

        public static UpdateWorldPositionMessage GenerateUpdateWorldPositionMsg()
        {
            if (_updateWorldPositionCache.Count > 0)
            {
                var message = _updateWorldPositionCache[0];
                _updateWorldPositionCache.Remove(message);
                return message;
            }

            return new UpdateWorldPositionMessage();
        }

        public static UseAbilityMessage GenerateUseAbilityMsg()
        {
            if (_useAbilityCache.Count > 0)
            {
                var message = _useAbilityCache[0];
                _useAbilityCache.Remove(message);
                return message;
            }

            return new UseAbilityMessage();
        }

        public static AddItemMessage GenerateAddItemMsg()
        {
            if (_addItemCache.Count > 0)
            {
                var message = _addItemCache[0];
                _addItemCache.Remove(message);
                return message;
            }

            return new AddItemMessage();
        }

        public static RemoveItemMessage GenerateRemoveItemMsg()
        {
            if (_removeItemCache.Count > 0)
            {
                var message = _removeItemCache[0];
                _removeItemCache.Remove(message);
                return message;
            }

            return new RemoveItemMessage();
        }

        public static QueryInventoryMessage GenerateQueryInventoryMsg()
        {
            if (_queryInventoryCache.Count > 0)
            {
                var message = _queryInventoryCache[0];
                _queryInventoryCache.Remove(message);
                return message;
            }

            return new QueryInventoryMessage();
        }

        public static AddGoldMessage GenerateAddGoldMsg()
        {
            if (_addGoldCache.Count > 0)
            {
                var message = _addGoldCache[0];
                _addGoldCache.Remove(message);
                return message;
            }

            return new AddGoldMessage();
        }

        public static RemoveGoldMessage GenerateRemoveGoldMsg()
        {
            if (_removeGoldCache.Count > 0)
            {
                var message = _removeGoldCache[0];
                _removeGoldCache.Remove(message);
                return message;
            }

            return new RemoveGoldMessage();
        }

        public static QueryGoldMessage GenerateQueryGoldMsg()
        {
            if (_queryGoldCache.Count > 0)
            {
                var message = _queryGoldCache[0];
                _queryGoldCache.Remove(message);
                return message;
            }

            return new QueryGoldMessage();
        }

        public static QueryLoadoutMessage GenerateQueryLoadoutMsg()
        {
            if (_queryLoadoutCache.Count > 0)
            {
                var message = _queryLoadoutCache[0];
                _queryLoadoutCache.Remove(message);
                return message;
            }

            return new QueryLoadoutMessage();
        }

        public static QueryItemsMessage GenerateQueryItemsMessage()
        {
            if (_queryItemsCache.Count > 0)
            {
                var message = _queryItemsCache[0];
                _queryItemsCache.Remove(message);
                return message;
            }

            return new QueryItemsMessage();
        }

        public static HasItemsQueryMessage GenerateHasItemsQueryMsg()
        {
            if (_hasItemsCache.Count > 0)
            {
                var message = _hasItemsCache[0];
                _hasItemsCache.Remove(message);
                return message;
            }

            return new HasItemsQueryMessage();
        }

        public static EquipItemToLoadoutSlotMessage GeneratEquipItemToLoadoutSlotMsg()
        {
            if (_equipItemToLoadoutSlotCache.Count > 0)
            {
                var message = _equipItemToLoadoutSlotCache[0];
                _equipItemToLoadoutSlotCache.Remove(message);
                return message;
            }

            return new EquipItemToLoadoutSlotMessage();
        }

        public static EquipAbilityToLoadoutSlotMessage GenerateEquipAbilityToLoadoutSlotMsg()
        {
            if (_equipAbilityToLoadoutSlotCache.Count > 0)
            {
                var message = _equipAbilityToLoadoutSlotCache[0];
                _equipAbilityToLoadoutSlotCache.Remove(message);
                return message;
            }

            return new EquipAbilityToLoadoutSlotMessage();
        }

        public static QueryAbilitiesMessage GenerateQueryAbilitiesMsg()
        {
            if (_queryAbilitiesCache.Count > 0)
            {
                var message = _queryAbilitiesCache[0];
                _queryAbilitiesCache.Remove(message);
                return message;
            }

            return new QueryAbilitiesMessage();
        }

        public static UnequipLoadoutSlotMessage GenerateUnequipLoadoutSlotMsg()
        {
            if (_unequipLoadoutSlotCache.Count > 0)
            {
                var message = _unequipLoadoutSlotCache[0];
                _unequipLoadoutSlotCache.Remove(message);
                return message;
            }

            return new UnequipLoadoutSlotMessage();
        }

        public static QueryTargetMessage GenerateQueryTargetMsg()
        {
            if (_queryTargetCache.Count > 0)
            {
                var message = _queryTargetCache[0];
                _queryTargetCache.Remove(message);
                return message;
            }

            return new QueryTargetMessage();
        }

        public static SetPathMessage GenerateSetPathMsg()
        {
            if (_setPathCache.Count > 0)
            {
                var message = _setPathCache[0];
                _setPathCache.Remove(message);
                return message;
            }

            return new SetPathMessage();
        }

        public static QueryWorldPositionMessage GenerateQueryWorldPositionMsg()
        {
            if (_queryWorldPositionCache.Count > 0)
            {
                var message = _queryWorldPositionCache[0];
                _queryWorldPositionCache.Remove(message);
                return message;
            }
            return new QueryWorldPositionMessage();
        }

        public static ApplyAspectRanksMessage GenerateApplyAspectRanksMsg()
        {
            if (_applyAspectRanksCache.Count > 0)
            {
                var message = _applyAspectRanksCache[0];
                _applyAspectRanksCache.Remove(message);
                return message;
            }

            return new ApplyAspectRanksMessage();
        }

        public static QueryAspectsMessage GenerateQueryAspectsMsg()
        {
            if (_queryAspectsCache.Count > 0)
            {
                var message = _queryAspectsCache[0];
                _queryAspectsCache.Remove(message);
                return message;
            }

            return new QueryAspectsMessage();
        }

        public static AddAspectMessage GenerateAddAspectMsg()
        {
            if (_addAspectCache.Count > 0)
            {
                var message = _addAspectCache[0];
                _addAspectCache.Remove(message);
                return message;
            }

            return new AddAspectMessage();
        }

        public static QueryExperienceMessage GenerateQueryExperienceMsg()
        {
            if (_queryExperienceCache.Count > 0)
            {
                var message = _queryExperienceCache[0];
                _queryExperienceCache.Remove(message);
                return message;
            }

            return new QueryExperienceMessage();
        }

        public static ApplyCombatStatsMessage GenerateApplyCombatStatsMsg()
        {
            if (_applyCombatStatsCache.Count > 0)
            {
                var message = _applyCombatStatsCache[0];
                _applyCombatStatsCache.Remove(message);
                return message;
            }

            return new ApplyCombatStatsMessage();
        }

        public static EquipArmorItemMessage GenerateEquipArmorItemMsg()
        {
            if (_equipArmorItemCache.Count > 0)
            {
                var message = _equipArmorItemCache[0];
                _equipArmorItemCache.Remove(message);
                return message;
            }

            return new EquipArmorItemMessage();
        }

        public static UnequipArmorItemFromSlotMessage GenerateUnequipArmorItemFromSlotMsg()
        {
            if (_unequipArmorItemFromSlotCache.Count > 0)
            {
                var message = _unequipArmorItemFromSlotCache[0];
                _unequipArmorItemFromSlotCache.Remove(message);
                return message;
            }

            return new UnequipArmorItemFromSlotMessage();
        }

        public static QueryArmorEquipmentMessage GenerateQueryArmorEquipmentMsg()
        {
            if (_queryArmoyEquipmentCache.Count > 0)
            {
                var message = _queryArmoyEquipmentCache[0];
                _queryArmoyEquipmentCache.Remove(message);
                return message;
            }

            return new QueryArmorEquipmentMessage();
        }

        public static QueryAbilityCooldownMessage GenerateQueryAbilityCooldownMsg()
        {
            if (_queryAbilityCooldownCache.Count > 0)
            {
                var message = _queryAbilityCooldownCache[0];
                _queryAbilityCooldownCache.Remove(message);
                return message;
            }

            return new QueryAbilityCooldownMessage();
        }

        public static QueryShopMessage GenerateQueryShopMsg()
        {
            if (_queryShopCache.Count > 0)
            {
                var message = _queryShopCache[0];
                _queryShopCache.Remove(message);
                return message;
            }

            return new QueryShopMessage();
        }

        public static SetInteractionMessage GenerateSetInteractionMsg()
        {
            if (_setInteractionCache.Count > 0)
            {
                var message = _setInteractionCache[0];
                _setInteractionCache.Remove(message);
                return message;
            }

            return new SetInteractionMessage();
        }

        public static RemoveInteractionMessage GenerateRemoveInteractionMsg()
        {
            if (_removeInteractionCache.Count > 0)
            {
                var message = _removeInteractionCache[0];
                _removeInteractionCache.Remove(message);
                return message;
            }

            return new RemoveInteractionMessage();
        }

        public static SetWorldPositionMessage GenerateSetWorldPositionMsg()
        {
            if (_setworldPositionCache.Count > 0)
            {
                var message = _setworldPositionCache[0];
                _setworldPositionCache.Remove(message);
                return message;
            }

            return new SetWorldPositionMessage();
        }

        public static HealMessage GenerateHealMsg()
        {
            if (_healCache.Count > 0)
            {
                var message = _healCache[0];
                _healCache.Remove(message);
                return message;
            }

            return new HealMessage();
        }

        public static SetDialogueMessage GenerateSetDialogueMsg()
        {
            if (_setDialogueCache.Count > 0)
            {
                var message = _setDialogueCache[0];
                _setDialogueCache.Remove(message);
                return message;
            }

            return new SetDialogueMessage();
        }

        public static QueryDamageBonusMessage GenerateQueryDamageBonusMsg()
        {
            if (_queryDamageBonusCache.Count > 0)
            {
                var message = _queryDamageBonusCache[0];
                _queryDamageBonusCache.Remove(message);
                return message;
            }

            return new QueryDamageBonusMessage();
        }


        public static ApplySecondaryStatsMessage GenerateApplySecondaryStatsMsg()
        {
            if (_applySecondaryStatsCache.Count > 0)
            {
                var message = _applySecondaryStatsCache[0];
                _applySecondaryStatsCache.Remove(message);
                return message;
            }

            return new ApplySecondaryStatsMessage();
        }

        public static QueryAvailableResourceUsesMessage GenerateQueryAvailableResourceUsesMsg()
        {
            if (_queryAvailableResourceUsesCache.Count > 0)
            {
                var message = _queryAvailableResourceUsesCache[0];
                _queryAvailableResourceUsesCache.Remove(message);
                return message;
            }

            return new QueryAvailableResourceUsesMessage();
        }

        //TODO: Start Cache

        public static void CacheMessage(AddTraitToUnitMessage msg)
        {
            msg.Trait = null;
            msg.DoAfter = null;
            msg.WorldId = null;
            msg.Sender = null;
            _addTraitToUnitCache.Add(msg);
        }

        public static void CacheMessage(RemoveTraitFromUnitMessage msg)
        {
            msg.Trait = null;
            msg.Sender = null;
            _removeTraitFromUnitCache.Add(msg);
        }

        public static void CacheMessage(RemoveTraitFromUnitByControllerMessage msg)
        {
            msg.Controller = null;
            msg.Sender = null;
            _removeTraitFromUnitByControllerCache.Add(msg);
        }

        public static void CacheMessage(HitboxCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _hitboxCheckCache.Add(msg);
        }

        public static void CacheMessage(EnterCollisionWithObjectMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _enterCollisiongWithObjectCache.Add(msg);
        }

        public static void CacheMessage(ExitCollisionWithObjectMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _exitCollisionWithObjectCache.Add(msg);
        }

        public static void CacheMessage(TraitCheckMessage msg)
        {
            msg.DoAfter = null;
            msg.TraitsToCheck = null;
            msg.Sender = null;
            _traitCheckCache.Add(msg);
        }

        public static void CacheMessage(RegisterCollisionMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _registerCollisionCache.Add(msg);
        }

        public static void CacheMessage(UnregisterCollisionMessage msg)
        {
            msg.Object = null;
            msg.Sender = null;
            _unregisterCollisionCache.Add(msg);
        }

        public static void CacheMessage(SetPositionMessage msg)
        {
            msg.Position = Vector2.zero;
            msg.Sender = null;
            _setPositionCache.Add(msg);
        }

        public static void CacheMessage(UpdatePositionMessage msg)
        {
            msg.Position = Vector2.zero;
            msg.Sender = null;
            _updatePositionCache.Add(msg);
        }

        public static void CacheMessage(QueryPositionMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryPositionCache.Add(msg);
        }

        public static void CacheMessage(SetDirectionMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.Sender = null;
            _setDirectionCache.Add(msg);
        }

        public static void CacheMessage(UpdateDirectionMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.Sender = null;
            _updateDirectionCache.Add(msg);
        }

        public static void CacheMessage(QueryDirectionMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryDirectionCache.Add(msg);
        }

        public static void CacheMessage(SetUnitAnimationStateMessage msg)
        {
            msg.State = UnitAnimationState.Idle;
            msg.Sender = null;
            _setUnitAnimationStateCache.Add(msg);
        }

        public static void CacheMessage(TakeDamageMessage msg)
        {
            msg.Event = false;
            msg.Amount = 0;
            msg.Sender = null;
            _takeDamageCache.Add(msg);
        }

        public static void CacheMessage(SetOwnerMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _setOwnerCache.Add(msg);
        }

        public static void CacheMessage(UpdateOwnerMessage msg)
        {
            msg.Owner = null;
            msg.Sender = null;
            _updateOwnerCache.Add(msg);
        }

        public static void CacheMessage(QueryOwnerMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryOwnerCache.Add(msg);
        }

        public static void CacheMessage(QueryCombatStatsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(ApplyFlashColorFxMessage msg)
        {
            msg.Color = Color.white;
            msg.FramesBetweenFlashes = 0;
            msg.Loops = 0;
            msg.Sender = null;
            _applyFlashColorFxCache.Add(msg);
        }

        public static void CacheMessage(UpdateKnockbackStateMessage msg)
        {
            msg.Active = false;
            msg.Sender = null;
            _updateKnockbackStateCache.Add(msg);
        }

        public static void CacheMessage(ApplyKnockbackMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.Speed = 0;
            msg.Sender = null;
            _applyKnockbackCache.Add(msg);
        }

        public static void CacheMessage(SetUnitStateMessage msg)
        {
            msg.State = UnitState.Active;
            msg.Sender = null;
            _setUnitStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateUnitStateMessage msg)
        {
            msg.State = UnitState.Active;
            msg.Sender = null;
            _updateUnitStateCache.Add(msg);
        }

        public static void CacheMessage(QueryUnitStateMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryUnitStateCache.Add(msg);
        }

        public static void CacheMessage(WalledCheckMessage msg)
        {
            msg.Direction = Vector2.zero;
            msg.Speed = 0f;
            msg.Origin = Vector2.zero;
            msg.DoAfter = null;
            msg.Sender = null;
            _walledCheckCache.Add(msg);
        }

        public static void CacheMessage(UpdateAbilityStateMessage msg)
        {
            msg.State = AbilityState.Backswing;
            msg.Sender = null;
            _updateAbilityStateCache.Add(msg);
        }

        public static void CacheMessage(UpdateFacingDirectionMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.Sender = null;
            _updateFacingDirectionCache.Add(msg);
        }

        public static void CacheMessage(QueryFacingDirectionMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryFacingDirectionCache.Add(msg);
        }

        public static void CacheMessage(SetupProjectileMessage msg)
        {
            msg.Direction = Vector2Int.zero;
            msg.MoveSpeed = 0;
            msg.Sender = null;
            _setupProjectileCache.Add(msg);
        }

        public static void CacheMessage(UpdateWorldPositionMessage msg)
        {
            msg.Position = WorldVector2Int.Zero;
            msg.Sender = null;
            _updateWorldPositionCache.Add(msg);
        }

        public static void CacheMessage(UseAbilityMessage msg)
        {
            msg.Ability = null;
            msg.Ids = null;
            msg.Sender = null;
            _useAbilityCache.Add(msg);
        }

        public static void CacheMessage(AddItemMessage msg)
        {
            msg.Item = null;
            msg.Stack = 0;
            msg.Sender = null;
            _addItemCache.Add(msg);
        }

        public static void CacheMessage(RemoveItemMessage msg)
        {
            msg.Item = null;
            msg.Stack = 0;
            msg.Update = true;
            msg.Sender = null;
            _removeItemCache.Add(msg);
        }

        public static void CacheMessage(QueryInventoryMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryInventoryCache.Add(msg);
        }

        public static void CacheMessage(AddGoldMessage msg)
        {
            msg.Amount = 0;
            msg.Sender = null;
            _addGoldCache.Add(msg);
        }

        public static void CacheMessage(RemoveGoldMessage msg)
        {
            msg.Amount = 0;
            msg.Sender = null;
            _removeGoldCache.Add(msg);
        }

        public static void CacheMessage(QueryGoldMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryGoldCache.Add(msg);
        }

        public static void CacheMessage(QueryLoadoutMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryLoadoutCache.Add(msg);
        }

        public static void CacheMessage(QueryItemsMessage msg)
        {
            msg.Query = null;
            msg.DoAfter = null;
            msg.Sender = null;
            _queryItemsCache.Add(msg);
        }

        public static void CacheMessage(HasItemsQueryMessage msg)
        {
            msg.DoAfter = null;
            msg.Items = null;
            msg.Sender = null;
            _hasItemsCache.Add(msg);
        }

        public static void CacheMessage(EquipItemToLoadoutSlotMessage msg)
        {
            msg.Item = null;
            msg.Slot = 0;
            msg.Stack = 1;
            msg.Sender = null;
            _equipItemToLoadoutSlotCache.Add(msg);
        }

        public static void CacheMessage(EquipAbilityToLoadoutSlotMessage msg)
        {
            msg.Ability = null;
            msg.Sender = null;
            msg.Slot = 0;
            _equipAbilityToLoadoutSlotCache.Add(msg);
        }

        public static void CacheMessage(QueryAbilitiesMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryAbilitiesCache.Add(msg);
        }

        public static void CacheMessage(UnequipLoadoutSlotMessage msg)
        {
            msg.Slot = 0;
            msg.Sender = null;
            _unequipLoadoutSlotCache.Add(msg);
        }

        public static void CacheMessage(QueryTargetMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryTargetCache.Add(msg);
        }

        public static void CacheMessage(SetPathMessage msg)
        {
            msg.MoveSpeed = 0;
            msg.Path = null;
            _setPathCache.Add(msg);
        }

        public static void CacheMessage(QueryWorldPositionMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryWorldPositionCache.Add(msg);
        }

        public static void CacheMessage(ApplyAspectRanksMessage msg)
        {
            msg.Ranks = 0;
            msg.Aspect = null;
            msg.Bonus = false;
            msg.Update = false;
            msg.Sender = null;
            _applyAspectRanksCache.Add(msg);
        }

        public static void CacheMessage(QueryAspectsMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryAspectsCache.Add(msg);
        }

        public static void CacheMessage(AddAspectMessage msg)
        {
            msg.Aspect = null;
            msg.Sender = null;
            _addAspectCache.Add(msg);
        }

        public static void CacheMessage(QueryExperienceMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryExperienceCache.Add(msg);
        }

        public static void CacheMessage(ApplyCombatStatsMessage msg)
        {
            msg.Stats = new CombatStats();
            msg.Sender = null;
            _applyCombatStatsCache.Add(msg);
        }

        public static void CacheMessage(EquipArmorItemMessage msg)
        {
            msg.Item = null;
            msg.CharmSlot = 0;
            msg.Sender = null;
            _equipArmorItemCache.Add(msg);
        }

        public static void CacheMessage(UnequipArmorItemFromSlotMessage msg)
        {
            msg.Slot = ArmorSlot.Helm;
            msg.CharmSlot = 0;
            msg.Sender = null;
            _unequipArmorItemFromSlotCache.Add(msg);
        }

        public static void CacheMessage(QueryArmorEquipmentMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryArmoyEquipmentCache.Add(msg);
        }

        public static void CacheMessage(QueryAbilityCooldownMessage msg)
        {
            msg.Ability = null;
            msg.DoAfter = null;
            msg.Sender = null;
            _queryAbilityCooldownCache.Add(msg);
        }

        public static void CacheMessage(QueryShopMessage msg)
        {
            msg.DoAfter = null;
            msg.Sender = null;
            _queryShopCache.Add(msg);
        }

        public static void CacheMessage(SetInteractionMessage msg)
        {
            msg.Interaction = null;
            msg.Action = string.Empty;
            msg.Sender = null;
            _setInteractionCache.Add(msg);
        }

        public static void CacheMessage(RemoveInteractionMessage msg)
        {
            msg.Interaction = null;
            msg.Sender = null;
            _removeInteractionCache.Add(msg);
        }

        public static void CacheMessage(SetWorldPositionMessage msg)
        {
            msg.Position = WorldVector2Int.Zero;
            msg.Sender = null;
            _setworldPositionCache.Add(msg);
        }

        public static void CacheMessage(HealMessage msg)
        {
            msg.Amount = 0;
            msg.IsEvent = false;
            msg.Sender = null;
            _healCache.Add(msg);
        }

        public static void CacheMessage(SetDialogueMessage msg)
        {
            msg.Dialogue = null;
            msg.Sender = null;
            _setDialogueCache.Add(msg);
        }

        public static void CacheMessage(QueryDamageBonusMessage msg)
        {
            msg.Amount = 0;
            msg.DoAfter = null;
            msg.Type = DamageType.Physical;
            msg.Tags = null;
            msg.Sender = null;
            _queryDamageBonusCache.Add(msg);
        }

        public static void CacheMessage(ApplySecondaryStatsMessage msg)
        {
            msg.Stats = new SecondaryStats();
            msg.Sender = null;
            _applySecondaryStatsCache.Add(msg);
        }

        public static void CacheMessage(QueryAvailableResourceUsesMessage msg)
        {
            msg.DoAfter = null;
            msg.Items = null;
            msg.Sender = null;
            _queryAvailableResourceUsesCache.Add(msg);
        }
    }
}