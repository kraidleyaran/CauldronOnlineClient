using Assets.Resources.Ancible_Tools.Scripts.System;
using DG.Tweening;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Dialogue
{
    public class UiDialogueWindow : UiWindowBase
    {
        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Text _dialogueText;
        [SerializeField] private float _textSpeed;

        private string[] _currentDialogue = new string[0];
        private int _dialogueIndex = 0;

        private Tween _dialogueTween = null;
        private GameObject _owner = null;

        public void Setup(string[] dialogue, GameObject owner)
        {
            _owner = owner;            
            _currentDialogue = dialogue;
            _dialogueText.text = string.Empty;
            ShowNextDialogue();
            SubscribeToMessages();
        }

        private void ShowNextDialogue()
        {
            _dialogueText.text = string.Empty;
            var dialogue = _dialogueText.GetFormmatedTextLines(_currentDialogue[_dialogueIndex]).ToSingleString();
            _dialogueTween = _dialogueText.DOText(dialogue, _textSpeed).SetSpeedBased(true).SetEase(Ease.Linear).OnComplete(DialogueTweenCompleted);
        }

        private void DialogueTweenCompleted()
        {
            _dialogueTween = null;
            var dialogue = _dialogueText.GetFormmatedTextLines(_currentDialogue[_dialogueIndex]).ToSingleString();
            _dialogueText.text = dialogue;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.RightShoulder && msg.Current.RightShoulder || !msg.Previous.Green && msg.Current.Green)
            {
                if (_dialogueTween != null)
                {
                    _dialogueTween.Complete(true);
                }
                else if (_dialogueIndex < _currentDialogue.Length - 1)
                {
                    _dialogueIndex++;
                    ShowNextDialogue();
                }
                else
                {
                    UiWindowManager.CloseWindow(this);
                }
            }
            else if (!msg.Previous.Red && msg.Current.Red || !msg.Previous.PlayerMenu && msg.Current.PlayerMenu)
            {
                UiWindowManager.CloseWindow(this);
            }
            
        }

        public override void Close()
        {
            if (_owner)
            {
                gameObject.SendMessageTo(DialogueWindowClosedMessage.INSTANCE, _owner);
            }
            
            base.Close();
        }

        public override void Destroy()
        {
            if (_dialogueTween != null)
            {
                if (_dialogueTween.IsActive())
                {
                    _dialogueTween.Kill();
                }

                _dialogueTween = null;
            }
            base.Destroy();
        }
    }
}