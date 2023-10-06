using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiWindowBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public virtual bool Movable => true;
        public virtual bool Static => false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiWindowManager.SetHoveredWindow(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiWindowManager.RemoveHoveredWindow(this);
        }

        public virtual void Close()
        {
            
        }

        public virtual void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}