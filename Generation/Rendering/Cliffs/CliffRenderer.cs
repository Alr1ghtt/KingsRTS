using UnityEngine;
using UnityEngine.Tilemaps;

public class CliffRenderer : MonoBehaviour
{
    [SerializeField] private TilemapLayerSystem _layers;
    [SerializeField] private TileBase _cliffTile;

    public void Render(MapData map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                MapTile tile = map.GetTile(x, y);

                if (tile.Type != TileType.Cliff)
                    continue;

                Tilemap cliffLayer = _layers.GetCliffLayer(tile.Height);

                if (cliffLayer == null)
                    continue;

                cliffLayer.SetTile(
                    new Vector3Int(x, y, 0),
                    _cliffTile
                );
            }
        }
    }
}