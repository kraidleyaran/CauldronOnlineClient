using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiWindowManager : MonoBehaviour
    {
        private static UiWindowManager _instance = null;

        private UiWindowBase _hovered = null;

        private Dictionary<string, UiWindowBase> _windows = new Dictionary<string, UiWindowBase>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static void SetHoveredWindow(UiWindowBase window)
        {
            _instance._hovered = window;
        }

        public static void RemoveHoveredWindow(UiWindowBase window)
        {
            if (_instance._hovered && _instance._hovered == window)
            {
                _instance._hovered = null;
            }
        }

        public static T OpenWindow<T>(T baseWindow) where T : UiWindowBase
        {
            if (_instance._windows.TryGetValue(baseWindow.name, out var window))
            {
                return window as T;
            }

            window = Instantiate(baseWindow, _instance.transform);
            window.name = baseWindow.name;
            _instance._windows.Add(baseWindow.name, window);
            return (T) window;

        }

        public static bool CloseWindow(UiWindowBase window)
        {
            if (_instance._windows.TryGetValue(window.name, out var openWindow))
            {
                openWindow.Close();
                openWindow.Destroy();
                Destroy(openWindow.gameObject);
                _instance._windows.Remove(window.name);
                return true;
            }

            return false;
        }

        public static bool IsWindowOpen(string windowName)
        {
            return _instance._windows.ContainsKey(windowName);
        }
    }
}