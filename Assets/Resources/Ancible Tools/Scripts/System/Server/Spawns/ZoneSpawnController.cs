using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class ZoneSpawnController : MonoBehaviour
    {
        [SerializeField] protected internal SpriteTrait _overrideSprite;
        [SerializeField] protected internal ServerUnitTemplate _template;
        [SerializeField] protected internal ServerTrait[] _additionalTraits = new ServerTrait[0];
        [SerializeField] private bool _showAppearance = true;
        [SerializeField] private bool _startActive = true;

        [Header("Editor Settings")]
        [SerializeField] protected internal SpriteRenderer _spriteRenderer;

        public virtual ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var objectSpawnData = _template.GetData(!_overrideSprite);
            if (_additionalTraits.Length > 0)
            {
                var traits = objectSpawnData.Traits.ToList();
                traits.AddRange(_additionalTraits.Where(t => t).Select(t => t.name));
                objectSpawnData.Traits = traits.ToArray();
            }
            if (_overrideSprite)
            {
                var traits = objectSpawnData.Traits.ToList();
                traits.Add(_overrideSprite.name);
                objectSpawnData.Traits = traits.ToArray();
            }
            objectSpawnData.StartActive = _startActive;
            return new ZoneSpawnData {Spawn = objectSpawnData, Tile = tile, ShowAppearance = _showAppearance, StartActive = _startActive};
        }

        public virtual void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            if (_template)
            {
                var sprite = _overrideSprite ? _overrideSprite : _template.GetSprite();
                if (_spriteRenderer && _template && sprite)
                {
                    _spriteRenderer.sprite = sprite.Sprite;
                    _spriteRenderer.transform.SetLocalScaling(sprite.Scaling);
                    _spriteRenderer.transform.SetLocalPosition(sprite.Offset);
                }
            }
#endif
        }
    }
}