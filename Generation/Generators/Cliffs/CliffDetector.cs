using UnityEngine;

public class CliffDetector : MonoBehaviour
{
    [SerializeField] private float _threshold = 3f;

    public void Detect(MapData data)
    {
        for (int x = 0; x < data.Width; x++)
            for (int y = 1; y < data.Height; y++)
            {
                var tile = data.GetTile(x, y);
                var below = data.GetTile(x, y - 1);

                if (tile.Height > below.Height)
                    tile.Type = TileType.Cliff;
            }
    }
}