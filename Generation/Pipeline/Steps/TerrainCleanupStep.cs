using UnityEngine;

public class TerrainCleanupStep : IMapGenerationStep
{
    public void Execute(MapData map, MapGenerationConfig config)
    {
        int iterations = Mathf.Max(0, config.CleanupIterations);

        for (int i = 0; i < iterations; i++)
            Smooth(map);

        FillTinyHoles(map);
        RemoveSingleTiles(map);
    }

    private void Smooth(MapData map)
    {
        bool[,] result = new bool[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                int count = CountLandNeighbors(map, x, y);
                bool land = IsLand(map, x, y);

                if (count >= 5)
                    result[x, y] = true;
                else if (count <= 2)
                    result[x, y] = false;
                else
                    result[x, y] = land;
            }
        }

        Apply(map, result);
    }

    private void FillTinyHoles(MapData map)
    {
        bool[,] result = new bool[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                bool land = IsLand(map, x, y);
                int count = CountLandNeighbors(map, x, y);

                result[x, y] = land || count >= 7;
            }
        }

        Apply(map, result);
    }

    private void RemoveSingleTiles(MapData map)
    {
        bool[,] result = new bool[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                bool land = IsLand(map, x, y);
                int count = CountCardinalLandNeighbors(map, x, y);

                result[x, y] = land && count > 0;
            }
        }

        Apply(map, result);
    }

    private int CountLandNeighbors(MapData map, int x, int y)
    {
        int count = 0;

        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                if (ox == 0 && oy == 0)
                    continue;

                if (IsLand(map, x + ox, y + oy))
                    count++;
            }
        }

        return count;
    }

    private int CountCardinalLandNeighbors(MapData map, int x, int y)
    {
        int count = 0;

        if (IsLand(map, x + 1, y))
            count++;

        if (IsLand(map, x - 1, y))
            count++;

        if (IsLand(map, x, y + 1))
            count++;

        if (IsLand(map, x, y - 1))
            count++;

        return count;
    }

    private bool IsLand(MapData map, int x, int y)
    {
        if (!map.IsInside(x, y))
            return false;

        MapTile tile = map.GetTile(x, y);

        return tile != null && tile.IsLand;
    }

    private void Apply(MapData map, bool[,] mask)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                MapTile tile = map.GetTile(x, y);

                if (tile == null)
                {
                    tile = new MapTile(new Vector2Int(x, y), mask[x, y]);
                    map.SetTile(x, y, tile);
                }

                tile.IsLand = mask[x, y];
                tile.Height = mask[x, y] ? Mathf.Max(1, tile.Height) : 0;
                tile.Type = mask[x, y] ? TileType.Terrain : TileType.Empty;
                tile.RampDirection = RampDirection.None;
            }
        }
    }
}
