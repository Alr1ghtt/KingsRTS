using UnityEngine;

public class TerrainAutotiler
{
    public GrassTileType Resolve(MapData map, int x, int y)
    {
        int height = map.GetTile(x, y).Height;

        bool n = Solid(map, x, y + 1, height);
        bool s = Solid(map, x, y - 1, height);
        bool e = Solid(map, x + 1, y, height);
        bool w = Solid(map, x - 1, y, height);

        if (!n && w && e) return GrassTileType.Top;
        if (!s && w && e) return GrassTileType.Bottom;
        if (!w && n && s) return GrassTileType.Left;
        if (!e && n && s) return GrassTileType.Right;

        if (!n && !w) return GrassTileType.TopLeft;
        if (!n && !e) return GrassTileType.TopRight;
        if (!s && !w) return GrassTileType.BottomLeft;
        if (!s && !e) return GrassTileType.BottomRight;

        return GrassTileType.Center;
    }

    bool Solid(MapData map, int x, int y, int height)
    {
        if (!map.IsInside(x, y))
            return false;

        MapTile tile = map.GetTile(x, y);

        return tile.IsLand && tile.Height == height;
    }
}