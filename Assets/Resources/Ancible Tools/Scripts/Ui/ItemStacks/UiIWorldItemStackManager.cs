using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.ItemStacks
{
    public class UiIWorldItemStackManager : UiWindowBase
    {
        private static UiIWorldItemStackManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private UiWorldItemStackController _itemStackTemplate;
        [SerializeField] private RectTransform _mainTransform;

        private Dictionary<GameObject, UiWorldItemStackController> _controllers = new Dictionary<GameObject, UiWorldItemStackController>();


        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _mainTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            _mainTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
        }

        public static void Setup(GameObject owner, int stack)
        {
            if (!_instance._controllers.TryGetValue(owner, out var controller))
            {
                controller = Instantiate(_instance._itemStackTemplate, _instance._mainTransform);
                _instance._controllers.Add(owner, controller);
            }
            controller.Setup(stack, owner);
        }

        public static void Remove(GameObject owner)
        {
            if (_instance && _instance._controllers.TryGetValue(owner, out var controller))
            {
                controller.Destroy();
                Destroy(controller.gameObject);
                _instance._controllers.Remove(owner);
            }
        }
    }
}