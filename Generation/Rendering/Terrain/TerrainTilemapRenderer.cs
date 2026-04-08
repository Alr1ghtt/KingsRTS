using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainTilemapRenderer : MonoBehaviour
{
    [SerializeField] private TilemapLayerSystem _layers;
    [SerializeField] private MapGenerationConfig _config;

    [Header("Water")]
    [SerializeField] private TileBase _waterTile;

    [Header("Foam")]
    [SerializeField] private TileBase _foamTile;

    [Header("Cliff")]
    [SerializeField] private TileBase _cliffLeft;
    [SerializeField] private TileBase _cliffCenter;
    [SerializeField] private TileBase _cliffRight;
    [SerializeField] private TileBase _cliffSingle;

    [Header("Water Cliff")]
    [SerializeField] private TileBase _waterCliffLeft;
    [SerializeField] private TileBase _waterCliffCenter;
    [SerializeField] private TileBase _waterCliffRight;

    [Header("Shadow")]
    [SerializeField] private TileBase _shadowTile;

    [Header("Ramp")]
    [SerializeField] private TileBase _rampBottomEast;
    [SerializeField] private TileBase _rampTopEast;
    [SerializeField] private TileBase _rampBottomWest;
    [SerializeField] private TileBase _rampTopWest;

    [Header("Grass")]
    [SerializeField] private TileBase _center;
    [SerializeField] private TileBase _top;
    [SerializeField] private TileBase _bottom;
    [SerializeField] private TileBase _left;
    [SerializeField] private TileBase _right;
    [SerializeField] private TileBase _topLeft;
    [SerializeField] private TileBase _topRight;
    [SerializeField] private TileBase _bottomLeft;
    [SerializeField] private TileBase _bottomRight;

    [Header("Triple")]
    [SerializeField] private TileBase _tripleTop;
    [SerializeField] private TileBase _tripleBottom;
    [SerializeField] private TileBase _tripleLeft;
    [SerializeField] private TileBase _tripleRight;

    [Header("Single")]
    [SerializeField] private TileBase _single;

    [Header("Water Grass")]
    [SerializeField] private TileBase _w_center;
    [SerializeField] private TileBase _w_top;
    [SerializeField] private TileBase _w_bottom;
    [SerializeField] private TileBase _w_left;
    [SerializeField] private TileBase _w_right;
    [SerializeField] private TileBase _w_topLeft;
    [SerializeField] private TileBase _w_topRight;
    [SerializeField] private TileBase _w_bottomLeft;
    [SerializeField] private TileBase _w_bottomRight;

    [Header("Water Triple")]
    [SerializeField] private TileBase _w_tripleTop;
    [SerializeField] private TileBase _w_tripleBottom;
    [SerializeField] private TileBase _w_tripleLeft;
    [SerializeField] private TileBase _w_tripleRight;

    [Header("Water Single")]
    [SerializeField] private TileBase _w_single;

    private TerrainAutotiler autotiler = new();

    public void Render(MapData map)
    {
        FillWater(map);

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(x, y);

                if (!tile.IsLand)
                    continue;

                RenderBaseCliff(map, x, y);
                RenderGrass(map, x, y);
                RenderRamp(map, tile, x, y);
            }
        }
    }

    void RenderRamp(MapData map, MapTile tile, int x, int y)
    {
        if (tile.Type != TileType.Ramp)
            return;

        bool isTopPart = false;

        if (map.IsInside(x, y - 1))
        {
            var below = map.GetTile(x, y - 1);
            if (below != null && below.Type == TileType.Ramp)
                isTopPart = true;
        }

        TileBase t;

        if (tile.RampDirection == RampDirection.East)
            t = isTopPart ? _rampTopEast : _rampBottomEast;
        else
            t = isTopPart ? _rampTopWest : _rampBottomWest;

        int level = tile.Height;
        var cliffLayer = _layers.GetCliffLayer(level);

        cliffLayer.SetTile(new Vector3Int(x, y, 0), t);
    }

    void RenderBaseCliff(MapData map, int x, int y)
    {
        var tile = map.GetTile(x, y);

        if (!tile.IsLand)
            return;

        _layers.BaseCliff.SetTile(new Vector3Int(x, y, 0), _cliffCenter);
    }

    void FillWater(MapData map)
    {
        int border = _config.WaterBorder;

        for (int x = -border; x < map.Width + border; x++)
        {
            for (int y = -border; y < map.Height + border; y++)
            {
                _layers.Water.SetTile(new Vector3Int(x, y, 0), _waterTile);
            }
        }
    }
    bool IsIsolated(MapData map, int x, int y)
    {
        int h = map.GetTile(x, y).Height;

        return IsLower(map, x + 1, y, h) &&
               IsLower(map, x - 1, y, h) &&
               IsLower(map, x, y + 1, h) &&
               IsLower(map, x, y - 1, h);
    }

    bool IsLower(MapData map, int x, int y, int h)
    {
        if (!map.IsInside(x, y))
            return true;

        var t = map.GetTile(x, y);

        if (t == null)
            return true;

        return t.Height < h;
    }
    void RenderGrass(MapData map, int x, int y)
    {
        var tile = map.GetTile(x, y);

        if (tile.Type == TileType.Ramp)
            return;

        var layer = _layers.GetGroundLayer(tile.Height);

        var type = autotiler.Resolve(map, x, y);

        type = AdjustForRamp(map, x, y, type);

        bool useWater = tile.Height == 1 && HasWaterNeighbor(map, x, y);

        TileBase t = GetTile(type, useWater);

        layer.SetTile(new Vector3Int(x, y, 0), t);

        if (tile.Height >= 2)
        {
            HandleVerticalCliff(map, x, y, type);
        }
        else
        {
            if (HasWaterNeighbor(map, x, y))
                _layers.Foam.SetTile(new Vector3Int(x, y, 0), _foamTile);
        }
    }

    TileBase GetTile(GrassTileType type, bool water)
    {
        if (water)
        {
            return type switch
            {
                GrassTileType.Top => _w_top,
                GrassTileType.Bottom => _w_bottom,
                GrassTileType.Left => _w_left,
                GrassTileType.Right => _w_right,
                GrassTileType.TopLeft => _w_topLeft,
                GrassTileType.TopRight => _w_topRight,
                GrassTileType.BottomLeft => _w_bottomLeft,
                GrassTileType.BottomRight => _w_bottomRight,
                GrassTileType.TripleTop => _w_tripleTop,
                GrassTileType.TripleBottom => _w_tripleBottom,
                GrassTileType.TripleLeft => _w_tripleLeft,
                GrassTileType.TripleRight => _w_tripleRight,
                GrassTileType.Single => _w_single,
                _ => _w_center
            };
        }

        return type switch
        {
            GrassTileType.Top => _top,
            GrassTileType.Bottom => _bottom,
            GrassTileType.Left => _left,
            GrassTileType.Right => _right,
            GrassTileType.TopLeft => _topLeft,
            GrassTileType.TopRight => _topRight,
            GrassTileType.BottomLeft => _bottomLeft,
            GrassTileType.BottomRight => _bottomRight,
            GrassTileType.TripleTop => _tripleTop,
            GrassTileType.TripleBottom => _tripleBottom,
            GrassTileType.TripleLeft => _tripleLeft,
            GrassTileType.TripleRight => _tripleRight,
            GrassTileType.Single => _single,
            _ => _center
        };
    }

    bool HasWaterNeighbor(MapData map, int x, int y)
    {
        var tile = map.GetTile(x, y);
        int h = tile.Height;

        return IsWaterStrict(map, x + 1, y, h) ||
               IsWaterStrict(map, x - 1, y, h) ||
               IsWaterStrict(map, x, y + 1, h) ||
               IsWaterStrict(map, x, y - 1, h);
    }

    bool IsWaterStrict(MapData map, int x, int y, int h)
    {
        if (!map.IsInside(x, y))
            return false;

        var t = map.GetTile(x, y);

        if (t == null)
            return false;

        return !t.IsLand && t.Height < h;
    }
    bool IsRamp(MapData map, int x, int y)
    {
        if (!map.IsInside(x, y))
            return false;

        var t = map.GetTile(x, y);
        return t != null && t.Type == TileType.Ramp;
    }
    GrassTileType AdjustForRamp(MapData map, int x, int y, GrassTileType original)
    {
        bool leftRamp = IsRamp(map, x - 1, y);
        bool rightRamp = IsRamp(map, x + 1, y);
        bool downRamp = IsRamp(map, x, y - 1);
        bool upRamp = IsRamp(map, x, y + 1);

        // только мягкая коррекция снизу
        if (downRamp)
            return GrassTileType.Bottom;

        if (upRamp)
            return GrassTileType.Top;

        return original;
    }
    bool IsWater(MapData map, int x, int y)
    {
        if (!map.IsInside(x, y))
            return false;

        var t = map.GetTile(x, y);

        if (t == null)
            return false;

        return !t.IsLand;
    }

    void HandleVerticalCliff(MapData map, int x, int y, GrassTileType type)
    {
        var sourceTile = map.GetTile(x, y);

        if (sourceTile.Type == TileType.Ramp)
            return;

        int startHeight = sourceTile.Height;

        bool allowWaterCliff = startHeight >= 2;

        int currentHeight = startHeight;
        int checkY = y - 1;

        while (true)
        {
            bool inside = map.IsInside(x, checkY);
            int belowHeight = inside ? map.GetTile(x, checkY).Height : 0;

            if (belowHeight >= currentHeight)
                break;

            int level = currentHeight;

            var cliffLayer = _layers.GetCliffLayer(level);
            var shadowLayer = _layers.GetShadowLayer(level);

            Vector3Int pos = new Vector3Int(x, checkY, 0);

            bool isLast = belowHeight == 0;

            TileBase cliff = ResolveCliffTile(map, x, y, type, level, isLast && allowWaterCliff);

            cliffLayer.SetTile(pos, cliff);
            shadowLayer.SetTile(pos, _shadowTile);

            if (isLast)
            {
                if (allowWaterCliff)
                    _layers.Foam.SetTile(pos, _foamTile);

                break;
            }

            currentHeight--;
            checkY--;
        }
    }

    TileBase ResolveCliffTile(MapData map, int x, int y, GrassTileType type, int level, bool waterVariant)
    {
        int height = map.GetTile(x, y).Height;

        bool leftSolid = map.IsInside(x - 1, y) &&
                         map.GetTile(x - 1, y).Height >= height;

        bool rightSolid = map.IsInside(x + 1, y) &&
                          map.GetTile(x + 1, y).Height >= height;

        if (type == GrassTileType.TripleBottom)
            return _cliffSingle;

        if (type == GrassTileType.TripleRight)
            return waterVariant ? _waterCliffRight : _cliffRight;

        if (type == GrassTileType.TripleLeft)
            return waterVariant ? _waterCliffLeft : _cliffLeft;

        if (!leftSolid && !rightSolid)
            return _cliffSingle;

        if (!leftSolid)
            return waterVariant ? _waterCliffLeft : _cliffLeft;

        if (!rightSolid)
            return waterVariant ? _waterCliffRight : _cliffRight;

        return waterVariant ? _waterCliffCenter : _cliffCenter;
    }
}