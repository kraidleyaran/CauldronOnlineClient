using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.ItemStacks
{
    public class UiWorldItemStackController : MonoBehaviour
    {
        [SerializeField] private Text _stackText = null;
        [SerializeField] private Vector2 _offset = new Vector2(0,6f);

        private GameObject _owner = null;
        private bool _active = false;

        public void Setup(int stack, GameObject owner)
        {
            _owner = owner;
            _stackText.text = $"x{stack}";
            _active = true;
            SetPosition();
            SubscribeToMessages();
        }

        private void SetPosition()
        {
            if (_owner)
            {
                var ownerPos = CameraController.Camera.WorldToScreenPoint(_owner.transform.position).ToVector2();
                transform.SetLocalPosition(ownerPos + _offset);
            }

        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            SetPosition();
        }

        public void Destroy()
        {
            _active = false;
            gameObject.UnsubscribeFromAllMessages();
        }

        void OnDestroy()
        {
            if (_active)
            {
                gameObject.UnsubscribeFromAllMessages();
            }

            _owner = null;
        }
    }
}