using UnityEngine;

public class CliffBuilder : MonoBehaviour
{
    public void Build(MapData data)
    {
        for (int x = 0; x < data.Width; x++)
        {
            for (int y = 0; y < data.Height; y++)
            {
                MapTile tile = data.GetTile(x, y);

                if (tile.Type == TileType.Cliff)
                {
                    tile.Height = Mathf.RoundToInt(tile.Height);
                }
            }
        }
    }
}