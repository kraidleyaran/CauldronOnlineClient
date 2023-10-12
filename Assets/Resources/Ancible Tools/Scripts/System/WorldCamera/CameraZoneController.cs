using System.Collections.Generic;
using System.Linq;
using ConcurrentMessageBus;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera
{
    public class CameraZoneController : MonoBehaviour
    {
        [SerializeField] private STETilemap _tilemap;

        private Collider2D[] _colliders = new Collider2D[0];
        private List<Vector2Int> _gridPositions = new List<Vector2Int>();

        void Awake()
        {
            _colliders = gameObject.GetComponentsInChildren<Collider2D>();
            var pos = new Vector2Int(_tilemap.MinGridX, _tilemap.MinGridY);
            while (pos.y <= _tilemap.MaxGridY)
            {
                var tileData = _tilemap.GetTileData(pos.x, pos.y);
                if (tileData == 0)
                {
                    _gridPositions.Add(pos);

                }
                pos.x++;
                if (pos.x > _tilemap.MaxGridX)
                {
                    pos.x = _tilemap.MinGridX;
                    pos.y++;
                }
            }
            CameraController.AddZone(this);
            ObjectManager.RegisterObject(gameObject);
            
        }

        public bool ContainsPoint(Vector2 pos)
        {
            var gridPos = TilemapUtils.GetGridPositionInt(_tilemap, _tilemap.transform.InverseTransformPoint(pos).ToVector2());
            return _gridPositions.Contains(gridPos);
            //var colliders = _colliders.Count(c => c.OverlapPoint(pos));
            //return colliders > 0;
        }

        public Vector2 ClosestPos(Vector2 pos)
        {
            if (ContainsPoint(pos))
            {
                return pos;

            }
            if (_colliders.Length > 0)
            {
                return _colliders.Select(c => c.ClosestPoint(pos)).OrderBy(p => (pos - p).sqrMagnitude).First();
            }
            return transform.position.ToVector2();
        }

        public Vector2 GetCameraPosition(Vector2 pos)
        {
            if (ContainsPoint(pos))
            {
                return pos;

            }
            if (_colliders.Length > 0)
            {
                var returnPos = _colliders.Select(c => c.ClosestPoint(pos)).OrderBy(p => (pos - p).sqrMagnitude).First();
                return returnPos;
            }

            return transform.position.ToVector2();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
            CameraController.RemoveZone(this);
        }
    }
}