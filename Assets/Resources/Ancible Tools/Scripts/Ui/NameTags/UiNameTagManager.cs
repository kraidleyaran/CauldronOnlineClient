using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using static UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.NameTags
{
    public class UiNameTagManager : UiWindowBase
    {
        private static UiNameTagManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private UiNameTagController _nameTagTemplate;

        private Dictionary<GameObject, UiNameTagController> _controllers = new Dictionary<GameObject, UiNameTagController>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
            SubscribeToMessages();
        }

        public static void GenerateNameTag(string name, GameObject owner)
        {
            if (!_instance._controllers.TryGetValue(owner, out var controller))
            {
                controller = Instantiate(_instance._nameTagTemplate, _instance.transform);
                _instance._controllers.Add(owner, controller);
            }

            controller.Setup(name, owner);
        }

        public static void RemoveNameTag(GameObject owner)
        {
            if (_instance && _instance._controllers.TryGetValue(owner, out var controller))
            {
                _instance._controllers.Remove(owner);
                controller.Destroy();
                Destroy(controller.gameObject);
            }
        }

        public static void Clear(bool clearPlayer)
        {
            var owners = clearPlayer ? _instance._controllers.ToArray() : _instance._controllers.Where(kv => kv.Key != ObjectManager.Player).ToArray();
            foreach (var owner in owners)
            {
                owner.Value.Destroy();
                Destroy(owner.Value.gameObject);
                _instance._controllers.Remove(owner.Key);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            if (msg.State == WorldState.Inactive || msg.State == WorldState.Inactive)
            {
                Clear(false);
            }
        }
    }
}