using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Server;
using Assets.Resources.Ancible_Tools.Scripts.System.Server.Spawns;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.Zones;
using CreativeSpore.SuperTilemapEditor;
using RogueSharp;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Zones
{
    public class WorldZoneController : MonoBehaviour
    {
        public const float PIXEL_SIZE = .3125f;

        [SerializeField] private STETilemap _pathing;

        private Dictionary<Vector2Int, ZoneTile> _tiles = new Dictionary<Vector2Int, ZoneTile>();

        private Vector2Int _offset = Vector2Int.zero;
        private Map _pathingMap = null;

        void Awake()
        {
            var size = new Vector2Int(_pathing.GridWidth, _pathing.GridHeight);
            var offset = new Vector2Int(_pathing.MinGridX, _pathing.MinGridY);

            if (offset.x < 0)
            {
                offset.x *= -1;
            }

            if (offset.y < 0)
            {
                offset.y *= -1;
            }
            _pathingMap = new Map(size.x, size.y);
            _offset = offset;
            var pos = new Vector2Int(_pathing.MinGridX, _pathing.MinGridY);
            var end = new Vector2Int(_pathing.MaxGridX + 1, _pathing.MaxGridY + 1);

            while (pos.y <= end.y)
            {
                if (_pathing.GetTileData(pos) == 0)
                {
                    if (!_tiles.ContainsKey(pos))
                    {
                        var cell = _pathingMap.GetCell(pos.x + offset.x, pos.y + offset.y);
                        _pathingMap.SetCellProperties(cell.X, cell.Y, true, true);
                        var centerPos = TilemapUtils.GetTileCenterPosition(_pathing, pos.x, pos.y) / PIXEL_SIZE;
                        _tiles.Add(pos, new ZoneTile{Cell = cell, Tile = pos, WorldCenter = centerPos});
                    }
                }

                pos.x++;
                if (pos.x > _pathing.MaxGridX)
                {
                    pos.x = _pathing.MinGridX;
                    pos.y++;
                }

            }
            Debug.Log($"{_tiles.Count} Tiles");
        }

        public WorldZoneData GetData(string zoneName, string[] aliases, WorldVector2Int defaultSpawn)
        {
            var size = new Vector2Int(_pathing.GridWidth, _pathing.GridHeight);
            var offset = new Vector2Int(_pathing.MinGridX, _pathing.MinGridY);

            if (offset.x < 0)
            {
                offset.x *= -1;
            }

            if (offset.y < 0)
            {
                offset.y *= -1;
            }

            var pos = new Vector2Int(_pathing.MinGridX, _pathing.MinGridY);
            var end = new Vector2Int(_pathing.MaxGridX + 1, _pathing.MaxGridY + 1);

            var tiles = new List<ZoneTileData>();

            while (pos.y <= end.y)
            {
                if (_pathing.GetTileData(pos) == 0)
                {
                    var centerPos = TilemapUtils.GetTileCenterPosition(_pathing, pos.x, pos.y) / PIXEL_SIZE;
                    tiles.Add(new ZoneTileData{Position = pos.ToWorldVector(), WorldPosition = new WorldVector2Int((int)centerPos.x,(int)centerPos.y)});
                }

                pos.x++;
                if (pos.x > _pathing.MaxGridX)
                {
                    pos.x = _pathing.MinGridX;
                    pos.y++;
                }

            }
            Debug.Log($"{tiles.Count} Tiles");

            var spawnControllers = gameObject.GetComponentsInChildren<ZoneSpawnController>();
            var zoneSpawns = new List<ZoneSpawnData>();
            foreach (var spawn in spawnControllers)
            {
                var tilePos = TilemapUtils.GetGridPositionInt(_pathing, spawn.transform.position.ToVector2()).ToWorldVector();
                if (tiles.Exists(t => t.Position == tilePos))
                {
                    zoneSpawns.Add(spawn.GetData(tilePos));
                }
            }
            var data = new WorldZoneData
            {
                Name = zoneName,
                Aliases = aliases,
                Size = size.ToWorldVector(),
                Offset = offset.ToWorldVector(),
                Tiles = tiles.ToArray(),
                Spawns = zoneSpawns.ToArray(),
                DefaultSpawn = defaultSpawn
            };

            return data;

        }

        public ZoneTile GetTileFromPixelPosition(WorldVector2Int pos)
        {
            var tilePos = TilemapUtils.GetGridPositionInt(pos.ToWorldVector(), _pathing.CellSize);
            if (_tiles.TryGetValue(tilePos, out var tile))
            {
                return tile;
            }

            return null;
        }

        public ZoneTile GetTileFromCell(ICell cell)
        {
            var tilePos = new Vector2Int(cell.X - _offset.x, cell.Y - _offset.y);
            if (_tiles.TryGetValue(tilePos, out var tile))
            {
                return tile;
            }

            return null;
        }

        public bool IsValidWorldPosition(Vector2 worldPos)
        {
            var tilePos = TilemapUtils.GetGridPositionInt(worldPos, _pathing.CellSize);
            return _tiles.ContainsKey(tilePos);
        }

        public bool IsInPov(WorldVector2Int originWorldPos, WorldVector2Int targetWorldPos, int area)
        {
            var originTile = GetTileFromPixelPosition(originWorldPos);
            var targetTile = GetTileFromPixelPosition(targetWorldPos);
            if (originTile != null && targetTile != null)
            {
                var cells = _pathingMap.ComputeFov(originTile.Cell.X, originTile.Cell.Y, area, true);
                var tiles = cells.Select(GetTileFromCell).Where(t => t != null).ToArray();
                return Array.IndexOf(tiles, targetTile) >= 0;
            }

            return false;
        }

        public PositionHelperController[] GetPlayerSpawns()
        {
            return gameObject.GetComponentsInChildren<PositionHelperController>().Where(p => p.IsPlayerSpawn).ToArray();
            
        }
    }
}