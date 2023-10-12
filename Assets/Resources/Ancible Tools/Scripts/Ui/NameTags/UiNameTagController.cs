using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.NameTags
{
    public class UiNameTagController : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Vector2 _offset = Vector2.zero;

        private GameObject _owner = null;

        public void Setup(string objectName, GameObject owner)
        {
            _owner = owner;
            _nameText.text = objectName;
            UpdatePosition();
            SubscribeToMessages();
        }

        private void UpdatePosition()
        {
            if (_owner)
            {
                var pos = CameraController.Camera.WorldToScreenPoint(_owner.transform.position.ToVector2()).ToVector2() + _offset;
                pos = new Vector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
                transform.SetTransformPosition(pos);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            UpdatePosition();
        }

        public void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}