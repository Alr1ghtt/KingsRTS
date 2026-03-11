using UnityEngine;
using UnityEngine.Tilemaps;

public class HeightBasedGroundRenderer : MonoBehaviour
{
    private readonly TilemapLayerSystem _layers;
    private readonly TileBase _groundTile;

    public HeightBasedGroundRenderer(TilemapLayerSystem layers, TileBase groundTile)
    {
        _layers = layers;
        _groundTile = groundTile;
    }

    public void Render(MapData data)
    {
        for (int x = 0; x < data.Width; x++)
        {
            for (int y = 0; y < data.Height; y++)
            {
                var tile = data.GetTile(x, y);

                if (!tile.IsLand)
                    continue;

                Tilemap layer = _layers.GetGroundLayer(tile.Height);

                if (layer == null)
                    continue;

                layer.SetTile(new Vector3Int(x, y, 0), _groundTile);
            }
        }
    }
}