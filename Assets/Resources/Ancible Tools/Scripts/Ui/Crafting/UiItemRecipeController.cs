using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Crafting
{
    public class UiItemRecipeController : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Text _stackText;
        [SerializeField] private RectTransform _cursorPosition;

        public ActiveRecipe Recipe;
        public Vector2Int Position;

        private bool _hovered = false;

        public void Setup(ActiveRecipe recipe)
        {
            Recipe = recipe;
            _frameImage.color = ItemFactory.GetQualityColor(Recipe.Item);
            _itemIcon.sprite = Recipe.Item.Sprite.Sprite;
            _stackText.text = $"x{recipe.Stack}";
        }

        public void SetHovered(bool hovered)
        {
            if (!_hovered && hovered)
            {
                UiHoverInfoManager.SetHoverInfo(gameObject, Recipe.Item.GetDisplayName(), Recipe.Item.GetDescription(), Recipe.Item.Sprite.Sprite, transform.position.ToVector2(), Recipe.Cost);
            }
            else if (_hovered && !hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }

            _hovered = hovered;
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
            UiCraftingWindow.ShowIngredients(Recipe.Recipe, Recipe.Cost);
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                UiHoverInfoManager.RemoveHoverInfo(gameObject);
            }
        }
    }
}