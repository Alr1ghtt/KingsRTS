using UnityEngine;
using UnityEngine.Tilemaps;

public class AutotileGroundRenderer : MonoBehaviour
{
    [SerializeField] private TilemapLayerSystem _layers;

    [SerializeField] private TileBase _centerTile;
    [SerializeField] private TileBase _edgeTile;
    [SerializeField] private TileBase _cornerTile;
    [SerializeField] private TileBase _singleTile;

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

                TerrainTileType type = _autotiler.Resolve(map, x, y);

                TileBase tileBase = GetTile(type);

                layer.SetTile(new Vector3Int(x, y, 0), tileBase);
            }
        }
    }

    private TileBase GetTile(TerrainTileType type)
    {
        switch (type)
        {
            case TerrainTileType.Center: return _centerTile;
            case TerrainTileType.Edge: return _edgeTile;
            case TerrainTileType.Corner: return _cornerTile;
            default: return _singleTile;
        }
    }
}