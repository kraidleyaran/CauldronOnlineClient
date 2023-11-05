using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon.Data.Math;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Tilemap Sprite Trait", menuName = "Ancible Tools/Traits/Animation/Tilemap Sprite")]
    public class TilemapSpriteTrait : Trait
    {
        public STETilemap Tilemap;
        public Vector2Int Offset = Vector2Int.zero;

        private STETilemap _tilemapController = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _tilemapController = Instantiate(Tilemap, _controller.transform.parent);
            _tilemapController.transform.SetLocalPosition(Offset.ToVector2(true));
        }

        public override void Destroy()
        {
            Destroy(_tilemapController.gameObject);
            base.Destroy();
        }
    }
}