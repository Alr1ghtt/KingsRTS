using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDebugRenderer : MonoBehaviour
{
    [SerializeField] private Tilemap _debugLayer;

    [SerializeField] private TileBase _heightTile;
    [SerializeField] private TileBase _cliffTile;
    [SerializeField] private TileBase _rampTile;

    public void Render(MapData map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                MapTile tile = map.GetTile(x, y);

                Vector3Int pos = new Vector3Int(x, y, 0);

                if (tile.Type == TileType.Cliff)
                {
                    _debugLayer.SetTile(pos, _cliffTile);
                }
                else if (tile.Type == TileType.Ramp)
                {
                    _debugLayer.SetTile(pos, _rampTile);
                }
                else if (tile.Height > 0)
                {
                    _debugLayer.SetTile(pos, _heightTile);
                }
            }
        }
    }
}