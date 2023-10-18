using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Items;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Crafting
{
    public class UiCraftingWindow : UiWindowBase
    {
        private static UiCraftingWindow _instance = null;

        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private UiRecipeManager _recipeManager;
        [SerializeField] private UiIngredientManager _ingredientManager;

        private GameObject _activeManager = null;
        private GameObject _owner = null;

        void Awake()
        {
            _instance = this;
        }

        public void Setup(ItemRecipeData[] recipes, GameObject owner)
        {
            _owner = owner;
            _recipeManager.Setup(recipes);
            _recipeManager.SetActive(true);
            _ingredientManager.SetActive(false);
            _activeManager = _recipeManager.gameObject;
            SubscribeToMessages();
        }

        public static void ShowIngredients(WorldItemStackData[] recipe, int cost)
        {
            _instance._ingredientManager.Setup(recipe, cost);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Green && msg.Current.Green)
            {
                if (_ingredientManager.CanCraft())
                {
                    _ingredientManager.RemoveItemsFromPlayer();
                    _recipeManager.CraftSelected();
                }
            }
            else if (!msg.Previous.Yellow && msg.Current.Yellow)
            {
                if (_activeManager == _recipeManager.gameObject)
                {
                    _activeManager = _ingredientManager.gameObject;
                    _ingredientManager.SetActive(true);
                    _recipeManager.SetActive(false);
                }
                else
                {
                    _activeManager = _recipeManager.gameObject;
                    _recipeManager.SetActive(true);
                    _ingredientManager.SetActive(false);
                }
            }
            else if (!msg.Previous.Red && msg.Current.Red || !msg.Previous.PlayerMenu && msg.Current.PlayerMenu)
            {
                UiWindowManager.CloseWindow(this);
            }
        }

        public override void Close()
        {
            _recipeManager.SetActive(false);
            _ingredientManager.SetActive(false);
            gameObject.SendMessageTo(CraftingWindowClosedMessage.INSTANCE, _owner);
            base.Close();
        }
    }
}