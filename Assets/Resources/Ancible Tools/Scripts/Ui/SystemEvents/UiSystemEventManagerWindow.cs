using System.Collections.Generic;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.SystemEvents
{
    public class UiSystemEventManagerWindow : UiWindowBase
    {
        private static UiSystemEventManagerWindow _instance = null;

        [SerializeField] private RectTransform _content;
        [SerializeField] private VerticalLayoutGroup _grid;
        [SerializeField] private UiSystemEventTextController _systemEventTemplate;
        [SerializeField] private int _maxEvents = 50;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Image[] _hoverImages = new Image[0];

        private List<UiSystemEventTextController> _controllers = new List<UiSystemEventTextController>();

        void Awake()
        {
            if (_instance)
            {
                UiWindowManager.CloseWindow(this);
                return;
            }

            _instance = this;
            SubscribeToMessages();
            foreach (var image in _hoverImages)
            {
                var color = image.color;
                color.a = 0;
                image.color = color;
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            foreach (var image in _hoverImages)
            {
                var color = image.color;
                color.a = .5f;
                image.color = color;
            }

            foreach (var controller in _controllers)
            {
                controller.Show();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            foreach (var image in _hoverImages)
            {
                var color = image.color;
                color.a = 0;
                image.color = color;
            }
        }

        public static void ShowSystemEvent(string message, SystemEventType type = SystemEventType.Key)
        {
            var controller = Instantiate(_instance._systemEventTemplate, _instance._content);
            controller.Setup(new SystemEvent{Message = message, Type = type});
            _instance._controllers.Add(controller);
            while (_instance._controllers.Count > _instance._maxEvents)
            {
                Destroy(_instance._controllers[0].gameObject);
                _instance._controllers.RemoveAt(0);
            }

            var size = _instance._controllers.Count * _instance._grid.spacing;
            foreach (var systemEvent in _instance._controllers)
            {
                size += systemEvent.RectTransform.rect.height;
            }

            _instance._content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            _instance._scrollRect.verticalScrollbar.value = 0f;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClientSystemEventMessage>(ClientSystemEvent);
        }

        private void ClientSystemEvent(ClientSystemEventMessage msg)
        {
            var systemEvent = msg.SystemEvent.GenerateSystemEvent();
            ShowSystemEvent(systemEvent.Message, systemEvent.Type);
        }
    }
}