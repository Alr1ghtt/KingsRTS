using UnityEngine;

public class TerrainAutotiler
{
    public TerrainTileType Resolve(MapData map, int x, int y)
    {
        MapTile tile = map.GetTile(x, y);

        if (!tile.IsLand)
            return TerrainTileType.Single;

        bool n = map.IsInside(x, y + 1) && map.GetTile(x, y + 1).IsLand;
        bool s = map.IsInside(x, y - 1) && map.GetTile(x, y - 1).IsLand;
        bool e = map.IsInside(x + 1, y) && map.GetTile(x + 1, y).IsLand;
        bool w = map.IsInside(x - 1, y) && map.GetTile(x - 1, y).IsLand;

        int count = 0;

        if (n) count++;
        if (s) count++;
        if (e) count++;
        if (w) count++;

        if (count == 4)
            return TerrainTileType.Center;

        if (count >= 2)
            return TerrainTileType.Edge;

        if (count == 1)
            return TerrainTileType.Corner;

        return TerrainTileType.Single;
    }
}