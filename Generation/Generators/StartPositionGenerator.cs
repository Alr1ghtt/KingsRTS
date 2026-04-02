using UnityEngine;

public class StartPositionGenerator
{
    public void Generate(MapData map, MapGenerationConfig config)
    {
        int count = config.PlayerCount;
        int radius = config.BaseRadius;

        float cx = map.Width / 2f;
        float cy = map.Height / 2f;

        float R = Mathf.Min(map.Width, map.Height) / 2f - radius;

        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = step * i * Mathf.Deg2Rad;

            int bx = Mathf.RoundToInt(cx + Mathf.Cos(angle) * R);
            int by = Mathf.RoundToInt(cy + Mathf.Sin(angle) * R);

            CreateBase(map, bx, by, radius);
        }
    }

    void CreateBase(MapData map, int cx, int cy, int radius)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (!map.IsInside(x, y))
                    continue;

                if (!CircleUtility.InsideCircle(x, y, cx, cy, radius))
                    continue;

                var tile = map.GetTile(x, y);

                tile.Height = 1;
                tile.Type = TileType.Terrain;
            }
        }
    }
}