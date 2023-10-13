using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using UnityEngine;
using static UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.FloatingText
{
    public class UiFloatingTextManager : UiWindowBase
    {
        private static UiFloatingTextManager _instance = null;

        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private RectTransform _transform;
        [SerializeField] private UiFloatingTextController _floatingTextTemplate;

        private List<UiFloatingTextController> _controllers = new List<UiFloatingTextController>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            _transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
        }

        public static void Register(string text, GameObject owner)
        {
            var pos = CameraController.Camera.WorldToScreenPoint(owner.transform.position.ToVector2());
            var controller = Instantiate(_instance._floatingTextTemplate, _instance.transform);
            controller.transform.SetTransformPosition(pos);
            controller.Setup(text, owner);
            _instance._controllers.Add(controller);
        }

        public static void Unregister(UiFloatingTextController controller)
        {
            _instance._controllers.Remove(controller);
            Destroy(controller.gameObject);
        }

        public static void Clear()
        {
            foreach (var controller in _instance._controllers)
            {
                Destroy(controller.gameObject);
            }
            _instance._controllers.Clear();
        }
    }
}