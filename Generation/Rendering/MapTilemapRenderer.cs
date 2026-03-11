using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTilemapRenderer : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _terrainTile;
    [SerializeField] private TileBase _cliffTile;
    [SerializeField] private TileBase _rampTile;

    public Tilemap Tilemap => _tilemap;

    public void Render(MapData data)
    {
        _tilemap.ClearAllTiles();

        for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
            {
                MapTile tile = data.GetTile(x, y);

                TileBase tileBase = null;

                if (tile.Type == TileType.Terrain) tileBase = _terrainTile;
                if (tile.Type == TileType.Cliff) tileBase = _cliffTile;
                if (tile.Type == TileType.Ramp) tileBase = _rampTile;

                _tilemap.SetTile(new Vector3Int(x, y, 0), tileBase);
            }
    }
}