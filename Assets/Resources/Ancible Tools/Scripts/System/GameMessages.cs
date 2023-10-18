using System;
using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Aspects;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldInput;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.WorldEvents;
using DG.Tweening;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class UpdateTickMessage : EventMessage
    {
        public static UpdateTickMessage INSTANCE = new UpdateTickMessage();
    }

    public class FixedUpdateTickMessage : EventMessage
    {
        public static FixedUpdateTickMessage INSTANCE = new FixedUpdateTickMessage();
    }

    public class UpdateClientStateMessage : EventMessage
    {
        public WorldClientState State;
    }

    public class SetUnitAnimationStateMessage : EventMessage
    {
        public UnitAnimationState State;
    }

    public class SetPositionMessage : EventMessage
    {
        public Vector2 Position;
    }

    public class UpdatePositionMessage : EventMessage
    {
        public Vector2 Position;
    }

    public class QueryPositionMessage : EventMessage
    {
        public Action<Vector2> DoAfter;
    }

    public class SetDirectionMessage : EventMessage
    {
        public Vector2Int Direction;
    }

    public class UpdateDirectionMessage : EventMessage
    {
        public Vector2Int Direction;
    }

    public class QueryDirectionMessage : EventMessage
    {
        public Action<Vector2Int> DoAfter;
    }

    public class UpdateInputStateMessage : EventMessage
    {
        public WorldInputState Previous;
        public WorldInputState Current;
    }

    public class ApplyMovementEventMessage : EventMessage
    {
        public MovementEvent Event;
    }

    public class SetAggroRangeMessage : EventMessage
    {
        public int AggroRange;
    }

    public class TakeDamageMessage : EventMessage
    {
        public int Amount;
        public DamageType Type;
        public bool Event;
        public string OwnerId;
    }

    public class SetCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public CombatVitals Vitals;
        public bool Report;
    }

    public class SetHurtboxSizeMessage : EventMessage
    {
        public WorldVector2Int Size;
        public WorldVector2Int Offset;
        public bool Knockback;
    }

    public class SetupHitboxesMessage : EventMessage
    {
        public ApplyHitboxData[] Hitboxes;
    }

    public class SetOwnerMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class UpdateOwnerMessage : EventMessage
    {
        public GameObject Owner;
    }

    public class QueryOwnerMessage : EventMessage
    {
        public Action<GameObject> DoAfter;
    }

    public class QueryCombatStatsMessage : EventMessage
    {
        public Action<CombatStats, CombatStats, CombatVitals, SecondaryStats> DoAfter;
    }

    public class CombatStatsUpdatedMessage : EventMessage
    {
        public static CombatStatsUpdatedMessage INSTANCE = new CombatStatsUpdatedMessage();
    }

    public class ApplyFlashColorFxMessage : EventMessage
    {
        public Color Color;
        public int Loops;
        public int FramesBetweenFlashes;
    }

    public class UpdateKnockbackStateMessage : EventMessage
    {
        public bool Active;
    }

    public class ApplyKnockbackMessage : EventMessage
    {
        public int Speed;
        public int Distance;
        public Vector2Int Direction;
        public string OwnerId;
    }

    public class ApplyKnockbackEventMessage : EventMessage
    {
        public int Time;
        public WorldVector2Int Position;
    }

    public class SetUnitStateMessage : EventMessage
    {
        public UnitState State;
    }

    public class UpdateUnitStateMessage : EventMessage
    {
        public UnitState State;
    }

    public class QueryUnitStateMessage : EventMessage
    {
        public Action<UnitState> DoAfter;
    }

    public class WalledCheckMessage : EventMessage
    {
        public Vector2 Origin;
        public Vector2 Direction;
        public float Speed;
        public Action<Vector2, bool> DoAfter;
    }

    public class SetupKnockbackMessage : EventMessage
    {
        public bool ReceiveKnockback;
    }

    public class UpdateAbilityStateMessage : EventMessage
    {
        public AbilityState State;
    }

    public class UpdateFacingDirectionMessage : EventMessage
    {
        public Vector2Int Direction;
    }

    public class UseAbilityMessage : EventMessage
    {
        public WorldAbility Ability;
        public string[] Ids;
        public WorldVector2Int Position;
    }

    public class SetApplyAbilityMessage : EventMessage
    {
        public bool ApplyAbility;
    }

    public class SetFacingDirectionMessage : EventMessage
    {
        public Vector2Int Direction;
    }

    public class UpdateWorldPositionMessage : EventMessage
    {
        public WorldVector2Int Position;
    }

    public class UnitDeathMessage : EventMessage
    {
        public static UnitDeathMessage INSTANCE = new UnitDeathMessage();
    }

    public class ApplyUnitDeathEventMessage : EventMessage
    {
        public static ApplyUnitDeathEventMessage INSTANCE = new ApplyUnitDeathEventMessage();
    }

    public class IsMonsterMessage : EventMessage
    {
        public static IsMonsterMessage INSTANCE = new IsMonsterMessage();
    }

    public class SetupProjectileMessage : EventMessage
    {
        public int MoveSpeed;
        public Vector2 Direction;
        public Trait[] ApplyOnWall;
        public bool ReportPosition;
        public string WorldId;
        public bool StopOnWall;
        public bool Unregister;
    }

    public class QueryFacingDirectionMessage : EventMessage
    {
        public Action<Vector2Int> DoAfter;
    }

    public class SetWorldPositionMessage : EventMessage
    {
        public WorldVector2Int Position;
    }

    public class AddItemMessage : EventMessage
    {
        public WorldItem Item;
        public int Stack;
    }

    public class RemoveItemMessage : EventMessage
    {
        public WorldItem Item;
        public int Stack;
        public bool Update;
    }

    public class QueryInventoryMessage : EventMessage
    {
        public Action<ItemStack[]> DoAfter;
    }

    public class AddGoldMessage : EventMessage
    {
        public int Amount;
    }

    public class RemoveGoldMessage : EventMessage
    {
        public int Amount;
    }

    public class QueryGoldMessage : EventMessage
    {
        public Action<int> DoAfter;
    }

    public class FullHealMessage : EventMessage
    {
        public static FullHealMessage INSTANCE = new FullHealMessage();
    }

    public class PlayerCombatStatsUpdatedMessage : EventMessage
    {
        public static PlayerCombatStatsUpdatedMessage INSTANCE = new PlayerCombatStatsUpdatedMessage();
    }

    public class UpdateWorldStateMessage : EventMessage
    {
        public WorldState State;
    }

    public class QueryLoadoutMessage : EventMessage
    {
        public Action<LoadoutSlot[]> DoAfter;
    }

    public class PlayerLoadoutUpdatedMessage : EventMessage
    {
        public static PlayerLoadoutUpdatedMessage INSTANCE = new PlayerLoadoutUpdatedMessage();
    }

    public class PlayerGoldUpdatedMessage : EventMessage
    {
        public static PlayerGoldUpdatedMessage INSTANCE = new PlayerGoldUpdatedMessage();
    }

    public class PlayerInventoryUpdatedMessage : EventMessage
    {
        public static PlayerInventoryUpdatedMessage INSTANCE = new PlayerInventoryUpdatedMessage();
    }

    public class TogglePlayerMenuMessage : EventMessage
    {
        public static TogglePlayerMenuMessage INSTANCE = new TogglePlayerMenuMessage();
    }

    public class QueryItemsMessage : EventMessage
    {
        public Predicate<ItemStack> Query;
        public Action<ItemStack[]> DoAfter;
        public bool StackAll;
    }

    public class HasItemsQueryMessage : EventMessage
    {
        public ItemStack[] Items;
        public Action<bool> DoAfter;
    }

    public class EquipItemToLoadoutSlotMessage : EventMessage
    {
        public ActionItem Item;
        public int Slot;
        public int Stack;
    }

    public class EquipAbilityToLoadoutSlotMessage : EventMessage
    {
        public WorldAbility Ability;
        public int Slot;
    }

    public class QueryAbilitiesMessage : EventMessage
    {
        public Action<WorldAbility[]> DoAfter;
    }

    public class UnequipLoadoutSlotMessage : EventMessage
    {
        public int Slot;
    }

    public class SetCurrentAggroTargetMessage : EventMessage
    {
        public GameObject Target;
    }

    public class QueryTargetMessage : EventMessage
    {
        public Action<GameObject> DoAfter;
    }

    public class SetPathMessage : EventMessage
    {
        public WorldVector2Int[] Path;
        public int MoveSpeed;
    }

    public class QueryWorldPositionMessage : EventMessage
    {
        public Action<WorldVector2Int> DoAfter;
    }

    public class ApplyAspectRanksMessage : EventMessage
    {
        public int Ranks;
        public bool Bonus;
        public WorldAspect Aspect;
        public bool Update;
    }

    public class QueryAspectsMessage : EventMessage
    {
        public Action<WorldAspectInstance[], int> DoAfter;
    }

    public class AddAspectMessage : EventMessage
    {
        public WorldAspect Aspect;
    }

    public class AddExperienceMessage : EventMessage
    {
        public int Experience;
    }

    public class QueryExperienceMessage : EventMessage
    {
        //Level, Current Experience, Required Experience
        public Action<int, int, int> DoAfter;
    }

    public class PlayerExperienceUpdatedMessage : EventMessage
    {
        public static PlayerExperienceUpdatedMessage INSTANCE = new PlayerExperienceUpdatedMessage();
    }

    public class PlayerAspectsUpdatedMessage : EventMessage
    {
        public static PlayerAspectsUpdatedMessage INSTANCE = new PlayerAspectsUpdatedMessage();
    }

    public class ApplyCombatStatsMessage : EventMessage
    {
        public CombatStats Stats;
        public bool Bonus;
    }

    public class EquipArmorItemMessage : EventMessage
    {
        public ArmorItem Item;
        public int CharmSlot;
    }

    public class UnequipArmorItemFromSlotMessage : EventMessage
    {
        public ArmorSlot Slot;
        public int CharmSlot;
    }

    public class QueryArmorEquipmentMessage : EventMessage
    {
        public Action<EquippedArmorItemInstance[]> DoAfter;
    }

    public class PlayerEquipmentUpdatedMessage : EventMessage
    {
        public static PlayerEquipmentUpdatedMessage INSTANCE = new PlayerEquipmentUpdatedMessage();
    }

    public class QueryAbilityCooldownMessage : EventMessage
    {
        public WorldAbility Ability;
        public Action<Sequence> DoAfter;
    }

    public class SetupShopMessage : EventMessage
    {
        public ShopItemData[] Items;
        public HitboxData Hitbox;
    }

    public class QueryShopMessage : EventMessage
    {
        public Action<ShopItem[]> DoAfter;
    }

    public class SetInteractionMessage : EventMessage
    {
        public GameObject Interaction;
        public string Action;
    }

    public class RemoveInteractionMessage : EventMessage
    {
        public GameObject Interaction;
    }

    public class InteractMessage : EventMessage
    {
        public static InteractMessage INSTANCE = new InteractMessage();
    }

    public class ShopWindowClosedMessage : EventMessage
    {
        public static ShopWindowClosedMessage INSTANCE = new ShopWindowClosedMessage();
    }

    public class SetupTerrainMessage : EventMessage
    {
        public HitboxData Hitbox;
    }

    public class HealMessage : EventMessage
    {
        public string OwnerId;
        public int Amount;
        public bool IsEvent;
    }

    public class DialogueWindowClosedMessage : EventMessage
    {
        public static DialogueWindowClosedMessage INSTANCE = new DialogueWindowClosedMessage();
    }

    public class SetDialogueMessage : EventMessage
    {
        public string[] Dialogue;
        public string ActionText;
    }

    public class SetupDoorMessage : EventMessage
    {
        public bool Open;
        public WorldItemStackData[] RequiredItems;
        public float Rotation;
        public HitboxData Hitbox;
        public bool AllowOpenWithNoItems;
        public WorldVector2Int TrappedSpawnPosition;
    }

    public class SetupNetworkTriggerHitboxMessage : EventMessage
    {
        public HitboxData Hitbox;
        public string[] TriggerEvents;
    }

    public class SetDoorStateMessage : EventMessage
    {
        public bool Open;
    }

    public class SetLoadedAspectsMessage : EventMessage
    {
        public int AvailablePoints;
        public AspectData[] Aspects;
    }

    public class SetExperienceMessage : EventMessage
    {
        public int Experience;
        public int Level;
    }

    public class SetInventoryMessage : EventMessage
    {
        public ItemStackData[] Items;
        public int Gold;
    }

    public class SetArmorEquipmentMessage : EventMessage
    {
        public string[] Equipped;
    }

    public class SetLoadoutMessage : EventMessage
    {
        public LoadoutSlotData[] Loadout;
    }

    public class SetupSwitchMessage : EventMessage
    {
        public string Switch;
        public string[] Signals;
        public HitboxData Hitbox;
        public int CurrentSignal;
        public bool Locked;
        public bool LockOnInteract;
        public bool CombatInteractable;
    }

    public class SetSignalMessage : EventMessage
    {
        public int Signal;
        public bool Locked;
    }

    public class QueryDamageBonusMessage : EventMessage
    {
        public int Amount;
        public DamageType Type;
        public BonusTag[] Tags;
        public Action<int> DoAfter;
    }

    public class ApplySecondaryStatsMessage : EventMessage
    {
        public SecondaryStats Stats;
    }

    public class QueryAvailableResourceUsesMessage : EventMessage
    {
        public ResourceItemStack[] Items;
        public Action<int> DoAfter;
    }

    public class OpenChestMessage : EventMessage
    {
        public static OpenChestMessage INSTANCE = new OpenChestMessage();
    }

    public class SetupChestMessage : EventMessage
    {
        public string OpenSprite;
        public string CloseSprite;
        public bool Open;
        public HitboxData Hitbox;
    }

    public class FillLoadoutStackMessage : EventMessage
    {
        public ActionItem Item;
        public int Stack;
        public Action<int> DoAfter;
    }

    public class RemoveManaMessage : EventMessage
    {
        public int Amount;
    }

    public class CloseChestMessage : EventMessage
    {
        public static CloseChestMessage INSTANCE = new CloseChestMessage();
    }

    public class RestoreManaMessage : EventMessage
    {
        public int Amount;
    }

    public class SetNameTagMessage : EventMessage
    {
        public string Name;
    }

    public class LevelUpMessage : EventMessage
    {
        public static LevelUpMessage INSTANCE = new LevelUpMessage();
    }

    public class QueryTimerMessage : EventMessage
    {
        public Action DoAfter;
    }

    public class RefreshTimerMessage : EventMessage
    {
        public static RefreshTimerMessage INSTANCE = new RefreshTimerMessage();
    }

    public class UpdateSlotUsesMessage : EventMessage
    {
        public int Slot;
    }

    public class SetupZoneTransitionMessage : EventMessage
    {
        public string Zone { get; set; }
        public WorldVector2Int Position { get; set; }
        public float Rotation { get; set; }
    }

    public class CraftingWindowClosedMessage : EventMessage
    {
        public static CraftingWindowClosedMessage INSTANCE = new CraftingWindowClosedMessage();
    }

    public class SetupCrafterMessage : EventMessage
    {
        public ItemRecipeData[] Recipes;
        public HitboxData Hitbox;
    }

    public class QueryRecallMessage : EventMessage
    {
        public Action<WorldZone, WorldVector2Int> DoAfter;
    }

    public class RecallMessage : EventMessage
    {
        public static RecallMessage INSTANCE = new RecallMessage();
    }

    public class WaypointWindowClosedMessage : EventMessage
    {
        public bool Travelling;
    }

    public class RemoveActiveRecallMessage : EventMessage
    {
        public static RemoveActiveRecallMessage INSTANCE = new RemoveActiveRecallMessage();
    }

    public class SwapLoadoutSlotsMessage : EventMessage
    {
        public int SlotA;
        public int SlotB;
    }

    public class SetProjectileDirectionMessage : EventMessage
    {
        public Vector2 Direction;
    }

    public class ResetMaxDistanceCheckMessage : EventMessage
    {
        public static ResetMaxDistanceCheckMessage INSTANCE = new ResetMaxDistanceCheckMessage();
    }

    public class ProjectileReturnedMessage : EventMessage
    {
        public GameObject Projectile;
    }

    public class RegisterProjectileMessage : EventMessage
    {
        public string ProjectileName;
        public int MaxStack;
        public GameObject Projectile;
    }

    public class ProjectileAvailableCheckMessage : EventMessage
    {
        public string Projectile;
        public Action DoAfter;
    }

    public class ReturningToOwnerMessage : EventMessage
    {
        public static ReturningToOwnerMessage INSTANCE = new ReturningToOwnerMessage();
    }

    public class SetupBridgeMessage : EventMessage
    {
        public string TilemapSprite;
        public WorldVector2Int Size;
        public bool Active;
    }

    public class SetBridgeStateMessage : EventMessage
    {
        public bool Active;
    }
    
}