using UnityEngine;

public class CliffDetectionStep : IMapGenerationStep
{
    public void Execute(MapData map, MapGenerationConfig config)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(x, y);

                if (tile == null || tile.Height < 2)
                    continue;

                if (tile.Type == TileType.Ramp)
                    continue;

                if (!IsCliff(map, x, y))
                    continue;

                ApplyCliff(map, x, y);
            }
        }
    }

    bool IsCliff(MapData map, int x, int y)
    {
        var tile = map.GetTile(x, y);
        var below = map.GetTile(x, y - 1);

        if (tile == null || below == null)
            return false;

        return tile.Height > below.Height;
    }

    void ApplyCliff(MapData map, int x, int y)
    {
        int h = map.GetTile(x, y).Height;
        int cy = y - 1;

        while (map.IsInside(x, cy))
        {
            var t = map.GetTile(x, cy);

            if (t.Height >= h)
                break;

            if (t.Type == TileType.Ramp)
                break;

            t.Type = TileType.Cliff;

            h--;
            cy--;
        }
    }
}