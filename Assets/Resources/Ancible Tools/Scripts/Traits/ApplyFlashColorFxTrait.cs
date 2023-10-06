using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Flash Color Fx Trait", menuName = "Ancible Tools/Traits/Animation/Fx/Apply Flash Color")]
    public class ApplyFlashColorFxTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private Color _color = Color.white;
        [SerializeField] private int _framesBetweenFlashes = 4;
        [SerializeField] private int _loops = 3;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            var applyFlashColorFxMsg = MessageFactory.GenerateApplyFlashColorFxMsg();
            applyFlashColorFxMsg.Color = _color;
            applyFlashColorFxMsg.FramesBetweenFlashes = _framesBetweenFlashes;
            applyFlashColorFxMsg.Loops = _loops;
            _controller.gameObject.SendMessageTo(applyFlashColorFxMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyFlashColorFxMsg);
        }
    }
}