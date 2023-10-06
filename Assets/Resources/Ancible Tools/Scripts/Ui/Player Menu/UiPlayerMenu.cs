using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Player_Menu
{
    public class UiPlayerMenu : MonoBehaviour
    {
        public string DisplayName;

        public virtual void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}