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

    [Header("Shadow")]
    [SerializeField] private TileBase _shadowTile;

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

    TerrainAutotiler autotiler = new();

    public void Render(MapData map)
    {
        FillWater(map);

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                MapTile tile = map.GetTile(x, y);

                if (!tile.IsLand)
                    continue;

                RenderGrass(map, x, y);
                RenderEdges(map, x, y);
            }
        }
    }

    void FillWater(MapData map)
    {
        int border = _config.WaterBorder;

        for (int x = -border; x < map.Width + border; x++)
        {
            for (int y = -border; y < map.Height + border; y++)
            {
                _layers.Water.SetTile(
                    new Vector3Int(x, y, 0),
                    _waterTile
                );
            }
        }
    }

    void RenderGrass(MapData map, int x, int y)
    {
        MapTile tile = map.GetTile(x, y);

        Tilemap layer = _layers.GetGroundLayer(tile.Height);

        var type = autotiler.Resolve(map, x, y);

        TileBase t =
            type == GrassTileType.Top ? _top :
            type == GrassTileType.Bottom ? _bottom :
            type == GrassTileType.Left ? _left :
            type == GrassTileType.Right ? _right :
            type == GrassTileType.TopLeft ? _topLeft :
            type == GrassTileType.TopRight ? _topRight :
            type == GrassTileType.BottomLeft ? _bottomLeft :
            type == GrassTileType.BottomRight ? _bottomRight :
            _center;

        layer.SetTile(new Vector3Int(x, y, 0), t);
    }

    void RenderEdges(MapData map, int x, int y)
    {
        MapTile tile = map.GetTile(x, y);

        if (!map.IsInside(x, y - 1))
            return;

        MapTile below = map.GetTile(x, y - 1);

        if (tile.Height == 1 && below.Height == 0)
        {
            _layers.Foam.SetTile(
                new Vector3Int(x, y, 0),
                _foamTile
            );
        }

        if (tile.Height >= 2 && below.Height < tile.Height)
        {
            PlaceCliff(map, x, y);
        }
    }

    void PlaceCliff(MapData map, int x, int y)
    {
        int height = map.GetTile(x, y).Height;

        Tilemap cliffLayer = _layers.GetCliffLayer(height);
        Tilemap shadowLayer = _layers.GetShadowLayer(height);

        bool left = map.IsInside(x - 1, y) &&
                    map.GetTile(x - 1, y).Height >= height;

        bool right = map.IsInside(x + 1, y) &&
                     map.GetTile(x + 1, y).Height >= height;

        TileBase cliff =
            !left ? _cliffLeft :
            !right ? _cliffRight :
            _cliffCenter;

        Vector3Int cliffPos = new Vector3Int(x, y - 1, 0);

        cliffLayer.SetTile(cliffPos, cliff);

        shadowLayer.SetTile(cliffPos, _shadowTile);
    }
}