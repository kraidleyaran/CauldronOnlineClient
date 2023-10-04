using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class DialogueZoneSpawnController : ZoneSpawnController
    {
        [SerializeField] private string _actionText = "Talk";
        [SerializeField][TextArea(3,5)] private string[] _dialogue = new string[0];
        [SerializeField] private SpriteTrait _sprite = null;
        [SerializeField] private ServerHitbox _serverHitbox;

        public override ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var data = base.GetData(tile);
            data.Spawn.AddParameter(new DialogueParameter{Dialogue = _dialogue, Hitbox = _serverHitbox.GetData(), ActionText = _actionText});
            data.Spawn.AddTrait(_sprite.name);
            return data;
        }

        public override void RefreshEditorSprite()
        {
            if (_sprite)
            {
#if UNITY_EDITOR
                _spriteRenderer.sprite = _sprite.Sprite;
                _spriteRenderer.transform.SetLocalScaling(_sprite.Scaling);
                _spriteRenderer.transform.SetLocalPosition(_sprite.Offset);
#endif
            }
            else
            {
                base.RefreshEditorSprite();
            }
            
        }
    }
}