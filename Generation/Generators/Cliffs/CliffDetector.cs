using UnityEngine;

public class CliffDetector : MonoBehaviour
{
    [SerializeField] private float _threshold = 3f;

    public void Detect(MapData data)
    {
        for (int x = 0; x < data.Width; x++)
        {
            for (int y = 0; y < data.Height; y++)
            {
                MapTile tile = data.GetTile(x, y);

                if (x > 0)
                {
                    float diff = Mathf.Abs(tile.Height - data.GetTile(x - 1, y).Height);

                    if (diff > _threshold)
                        tile.Type = TileType.Cliff;
                }

                if (y > 0)
                {
                    float diff = Mathf.Abs(tile.Height - data.GetTile(x, y - 1).Height);

                    if (diff > _threshold)
                        tile.Type = TileType.Cliff;
                }
            }
        }
    }
}