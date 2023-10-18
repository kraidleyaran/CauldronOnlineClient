using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using Assets.Resources.Ancible_Tools.Scripts.Ui.Crafting;
using CauldronOnlineCommon.Data.Items;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Network Crafter Trait", menuName = "Ancible Tools/Traits/Network/Interactions/Network Crafter")]
    public class NetworkCrafterTrait : InteractableTrait
    {
        public const string CRAFT = "Craft";

        protected internal override string _actionText => CRAFT;

        private ItemRecipeData[] _recipes = new ItemRecipeData[0];

        private UiCraftingWindow _activeWindow = null;

        protected internal override void Interact()
        {
            if (!_activeWindow)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Interaction;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);

                _controller.StartCoroutine(StaticMethods.WaitForFrames(1, OpenCraftingWindow));
            }
        }

        private void OpenCraftingWindow()
        {
            _activeWindow = UiWindowManager.OpenWindow(UiController.Crafting);
            _activeWindow.Setup(_recipes, _controller.gameObject);
        }

        private void CraftingWindowClosed()
        {
            _activeWindow = null;
            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
            setUnitStateMsg.State = UnitState.Active;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.gameObject.SubscribeWithFilter<CraftingWindowClosedMessage>(CraftingWindowClosed, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupCrafterMessage>(SetupCrafter, _instanceId);
        }

        private void CraftingWindowClosed(CraftingWindowClosedMessage msg)
        {
            _controller.StartCoroutine(StaticMethods.WaitForFrames(1, CraftingWindowClosed));
        }

        private void SetupCrafter(SetupCrafterMessage msg)
        {
            _recipes = msg.Recipes.ToArray();
            _hitboxController.transform.SetLocalScaling(msg.Hitbox.Size.ToVector());
            _hitboxController.transform.SetLocalPosition(msg.Hitbox.Offset.ToWorldVector());
        }
    }
}