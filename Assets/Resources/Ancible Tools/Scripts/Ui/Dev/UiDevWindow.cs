using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Dev
{
    public class UiDevWindow : UiWindowBase
    {
        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private Text _cameraButtonText;

        void Awake()
        {
            _cameraButtonText.text = CameraController.Locked ? "Unlock Camera" : "Lock Camera";
        }
        
        public void LevelUp()
        {
            gameObject.SendMessageTo(LevelUpMessage.INSTANCE, ObjectManager.Player);
        }

        public void ToggleCameraLock()
        {
            CameraController.SetLockedState(!CameraController.Locked);
            _cameraButtonText.text = CameraController.Locked ? "Unlock Camera" : "Lock Camera";
        }

    }
}