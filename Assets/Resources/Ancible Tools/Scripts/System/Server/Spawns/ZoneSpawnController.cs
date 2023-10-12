using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.UnitTemplates;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Zones;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns
{
    public class ZoneSpawnController : MonoBehaviour
    {
        [SerializeField] protected internal ServerUnitTemplate _template;
        [SerializeField] protected internal ServerTrait[] _additionalTraits;
        [SerializeField] private bool _showAppearance = true;

        [Header("Editor Settings")]
        [SerializeField] protected internal SpriteRenderer _spriteRenderer;

        public virtual ZoneSpawnData GetData(WorldVector2Int tile)
        {
            var objectSpawnData = _template.GetData();
            if (_additionalTraits.Length > 0)
            {
                var traits = objectSpawnData.Traits.ToList();
                traits.AddRange(_additionalTraits.Where(t => t).Select(t => t.name));
                objectSpawnData.Traits = traits.ToArray();
            }

            return new ZoneSpawnData {Spawn = objectSpawnData, Tile = tile, ShowAppearance = _showAppearance};
        }

        public virtual void RefreshEditorSprite()
        {
#if UNITY_EDITOR
            if (_spriteRenderer && _template && _template.Sprite)
            {
                _spriteRenderer.sprite = _template.Sprite.Sprite;
                _spriteRenderer.transform.SetLocalScaling(_template.Sprite.Scaling);
                _spriteRenderer.transform.SetLocalPosition(_template.Sprite.Offset);
            }
#endif
        }
    }
}