using UnityEngine;

public class TerrainAutotiler
{
    bool Solid(MapData map, int x, int y, int height)
    {
        if (!map.IsInside(x, y))
            return false;

        var tile = map.GetTile(x, y);

        if (tile == null || !tile.IsLand)
            return false;

        if (tile.Type == TileType.Ramp)
            return false;

        return tile.Height == height;
    }

    public GrassTileType Resolve(MapData map, int x, int y)
    {
        int height = map.GetTile(x, y).Height;

        bool n = Solid(map, x, y + 1, height);
        bool s = Solid(map, x, y - 1, height);
        bool e = Solid(map, x + 1, y, height);
        bool w = Solid(map, x - 1, y, height);

        int count =
            (n ? 1 : 0) +
            (s ? 1 : 0) +
            (e ? 1 : 0) +
            (w ? 1 : 0);

        if (count == 0)
            return GrassTileType.Single;

        if (count == 1)
        {
            if (n) return GrassTileType.TripleBottom;
            if (s) return GrassTileType.TripleTop;
            if (e) return GrassTileType.TripleLeft;
            if (w) return GrassTileType.TripleRight;
        }

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
}
