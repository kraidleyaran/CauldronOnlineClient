using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Characters
{
    public class UiCreateCharacterWindow : UiWindowBase
    {
        private static UiCreateCharacterWindow _instance = null;

        [SerializeField] private UiAnimationController _playerSprite = null;
        [SerializeField] private InputField _nameInput = null;
        [SerializeField] private Button _createButton = null;

        [Header("Color Options")]
        [SerializeField] private UiColorOptionController _hairColorController;
        [SerializeField] private UiColorOptionController _eyeColorController;
        [SerializeField] private UiColorOptionController _primaryShirtColorController;
        [SerializeField] private UiColorOptionController _secondaryShirtColorController;


        void Awake()
        {
            _createButton.interactable = false;
            _playerSprite.WakeUp();
            _hairColorController.Setup(_playerSprite.Material);
            _eyeColorController.Setup(_playerSprite.Material);
            _primaryShirtColorController.Setup(_playerSprite.Material);
            _secondaryShirtColorController.Setup(_playerSprite.Material);
            SubscribeToMessages();
        }

        public void CreateCharacter()
        {
            if (!string.IsNullOrEmpty(_nameInput.text) && !DataController.DoesCharacterNameExist(_nameInput.text))
            {
                var colorData = new SpriteColorData
                {
                    Hair = _hairColorController.GetOption(),
                    Eyes = _eyeColorController.GetOption(),
                    PrimaryShirt = _primaryShirtColorController.GetOption(),
                    SecondaryShirt = _secondaryShirtColorController.GetOption()
                };
                DataController.GenerateNewCharacter(_nameInput.text, colorData);
                UiWindowManager.OpenWindow(UiController.CharacterManager);
                UiWindowManager.CloseWindow(this);
            }
        }

        public void NameInputUpdated()
        {
            _createButton.interactable = !string.IsNullOrEmpty(_nameInput.text);
        }

        public void Back()
        {
            UiWindowManager.OpenWindow(UiController.CharacterManager);
            UiWindowManager.CloseWindow(this);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateClientStateMessage>(UpdateClientState);
        }

        private void UpdateClientState(UpdateClientStateMessage msg)
        {
            if (msg.State == WorldClientState.Disconnected)
            {
                UiWindowManager.CloseWindow(this);
            }
        }

    }
}