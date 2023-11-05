using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiMenuItemController : MonoBehaviour
    {
        [SerializeField] private UnityEvent _action;
        [SerializeField] private RectTransform _cursorPosition;
        [SerializeField] private CanvasGroup _canvasGroup;

        public bool Active { get; private set; }

        void Awake()
        {
            Active = true;
        }

        public void SelectItem()
        {
            _action.Invoke();
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void SetActive(bool active)
        {
            Active = active;
            _canvasGroup.alpha = Active ? 1f : .66f;
        }
    }
}