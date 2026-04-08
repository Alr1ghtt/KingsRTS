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
    bool HasAdjacentRamp(MapData map, int x, int y)
    {
        return IsRamp(map, x + 1, y) ||
               IsRamp(map, x - 1, y) ||
               IsRamp(map, x, y + 1) ||
               IsRamp(map, x, y - 1);
    }

    bool IsRamp(MapData map, int x, int y)
    {
        if (!map.IsInside(x, y))
            return false;

        var t = map.GetTile(x, y);
        return t != null && t.Type == TileType.Ramp;
    }
    bool IsCliff(MapData map, int x, int y)
    {
        var tile = map.GetTile(x, y);
        var below = map.GetTile(x, y - 1);

        if (tile == null || below == null)
            return false;

        if (below.Type == TileType.Ramp)
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
            {
                cy--;
                h--;
                continue;
            }

            t.Type = TileType.Cliff;

            h--;
            cy--;
        }
    }
}