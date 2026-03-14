using UnityEngine;
using UnityEngine.Tilemaps;

public class AutotileGroundRenderer : MonoBehaviour
{
    [SerializeField] private TilemapLayerSystem _layers;

    [SerializeField] private TileBase _centerTile;
    [SerializeField] private TileBase _topTile;
    [SerializeField] private TileBase _bottomTile;
    [SerializeField] private TileBase _leftTile;
    [SerializeField] private TileBase _rightTile;
    [SerializeField] private TileBase _topLeftTile;
    [SerializeField] private TileBase _topRightTile;
    [SerializeField] private TileBase _bottomLeftTile;
    [SerializeField] private TileBase _bottomRightTile;

    private TerrainAutotiler _autotiler = new();

    public void Render(MapData map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                MapTile tile = map.GetTile(x, y);

                if (!tile.IsLand)
                    continue;

                Tilemap layer = _layers.GetGroundLayer(tile.Height);

                GrassTileType type = _autotiler.Resolve(map, x, y);

                TileBase tileBase = GetTile(type);

                layer.SetTile(new Vector3Int(x, y, 0), tileBase);
            }
        }
    }

    private TileBase GetTile(GrassTileType type)
    {
        switch (type)
        {
            case GrassTileType.Top: return _topTile;
            case GrassTileType.Bottom: return _bottomTile;
            case GrassTileType.Left: return _leftTile;
            case GrassTileType.Right: return _rightTile;
            case GrassTileType.TopLeft: return _topLeftTile;
            case GrassTileType.TopRight: return _topRightTile;
            case GrassTileType.BottomLeft: return _bottomLeftTile;
            case GrassTileType.BottomRight: return _bottomRightTile;
            default: return _centerTile;
        }
    }
}