using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiPromptWindow : UiWindowBase
    {
        [SerializeField] private Text _promptText;

        private Action<bool> _doAfter;

        public void Setup(string prompt, Action<bool> doAfter)
        {
            _doAfter = doAfter;
            _promptText.text = prompt;
        }

        public void Confirm()
        {
            _doAfter.Invoke(true);
        }

        public void Decline()
        {
            _doAfter.Invoke(false);
        }
    }
}