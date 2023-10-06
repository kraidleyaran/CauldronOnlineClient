using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.Ui;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Dialogue Trait", menuName = "Ancible Tools/Traits/Interactable/Dialogue")]
    public class DialogueTrait : InteractableTrait
    {
        [SerializeField][TextArea(3,5)] private string[] _dialogue;

        protected internal override string _actionText => _dialogueActionText;

        private string _dialogueActionText = "Talk";

        protected internal override void Interact()
        {
            var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
            setUnitStateMsg.State = UnitState.Interaction;
            _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitStateMsg);

            _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                UiController.ShowDialogue(_dialogue, _controller.transform.parent.gameObject);
            }));
        }

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<DialogueWindowClosedMessage>(DialogueWindowClosed, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetDialogueMessage>(SetDialogue, _instanceId);
        }

        private void DialogueWindowClosed(DialogueWindowClosedMessage msg)
        {
            _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
            {
                var setUnitStateMsg = MessageFactory.GenerateSetUnitStateMsg();
                setUnitStateMsg.State = UnitState.Active;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, ObjectManager.Player);
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }));

        }

        private void SetDialogue(SetDialogueMessage msg)
        {
            _dialogue = msg.Dialogue;
            _dialogueActionText = msg.ActionText;
        }
    }
}