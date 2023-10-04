using Assets.Resources.Ancible_Tools.Scripts.System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.HoverInfo
{
    public class UiHoverInfoManager : UiWindowBase
    {
        private static UiHoverInfoManager _instance = null;

        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private UiHoverInfoController _hoverInfo;
        [SerializeField] private Vector2 _offset = Vector2.zero;

        private GameObject _owner = null;

        void Awake()
        {
            if (_instance)
            {
                UiWindowManager.CloseWindow(this);
                return;
            }

            _instance = this;
            _hoverInfo.gameObject.SetActive(false);
        }

        public static void SetHoverInfo(GameObject owner, string title, string description, Sprite icon, Vector2 position, int amount = 0)
        {
            _instance._owner = owner;
            _instance._hoverInfo.gameObject.SetActive(true);
            _instance._hoverInfo.Setup(icon, title, description, amount);
            _instance._hoverInfo.SetPosition(position);
            var quadrant = StaticMethods.GetMouseQuadrant(position + _instance._offset);
            _instance._hoverInfo.SetPivot(quadrant);
            
        }

        public static void RemoveHoverInfo(GameObject owner)
        {
            if (_instance._owner && _instance._owner == owner)
            {
                _instance._owner = null;
                _instance._hoverInfo.Clear();
                _instance._hoverInfo.gameObject.SetActive(false);
            }
        }

    }
}