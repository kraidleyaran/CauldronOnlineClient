using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiServerStatusWindow : UiWindowBase
    {
        private static UiServerStatusWindow _instance = null;

        [SerializeField] private Text _statusText;
        [SerializeField] private Button _closeButton;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _instance.gameObject.SetActive(false);
        }

        public static void SetStatusText(string text, bool showButton = false)
        {
            _instance._statusText.text = text;
            _instance.gameObject.SetActive(true);
            _instance._closeButton.gameObject.SetActive(showButton);
        }

        public static void Clear()
        {
            _instance._statusText.text = string.Empty;
            _instance.gameObject.SetActive(false);
        }
    }
}