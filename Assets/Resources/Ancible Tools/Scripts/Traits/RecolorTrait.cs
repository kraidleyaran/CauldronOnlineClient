using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Recolor Trait", menuName = "Ancible Tools/Traits/Animation/Recolor")]
    public class RecolorTrait : Trait
    {
        public const string ORIGIN_COLOR = "_OriginColor";
        public const string CHANGE_COLOR = "_ChangeColor";

        [SerializeField] private Material _material = null;
        [SerializeField] private ColorChange[] _colors = new ColorChange[0];


        private Material _objMaterial = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            
            _objMaterial = Instantiate(_material);
            foreach (var color in _colors)
            {
                _objMaterial.SetColor(color.OriginProperty, color.Origin);
                _objMaterial.SetColor(color.DestinationProperty, color.Destination);
            }

            var setDefaultMaterialsMsg = MessageFactory.GenerateSetDefaulMaterialsMsg();
            setDefaultMaterialsMsg.Default = new []{ _objMaterial };
            _controller.gameObject.SendMessageTo(setDefaultMaterialsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setDefaultMaterialsMsg);
        }

        

        public override void Destroy()
        {
            if (_objMaterial)
            {
                Destroy(_objMaterial);
            }

            _objMaterial = null;
            base.Destroy();
        }
    }
}