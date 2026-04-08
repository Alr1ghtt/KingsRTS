using System.Collections.Generic;
using UnityEngine;

public class RampGenerationStep : IMapGenerationStep
{
    private const int MIN_PLATFORM_SIZE = 4;
    private const int BORDER_OFFSET = 1;

    public void Execute(MapData map, MapGenerationConfig config)
    {
        var visited = new bool[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (visited[x, y])
                    continue;

                var tile = map.GetTile(x, y);

                if (tile == null || tile.Height < 2)
                    continue;

                var platform = FloodFill(map, x, y, tile.Height, visited);

                if (platform.Count < MIN_PLATFORM_SIZE)
                    continue;

                var candidates = FindEdgeCandidates(map, platform);

                if (candidates.Count == 0)
                    continue;

                PlacePlatformRamps(map, candidates);
            }
        }

        GenerateDepressionRamps(map);
    }

    void GenerateDepressionRamps(MapData map)
    {
        var visited = new bool[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (visited[x, y])
                    continue;

                var tile = map.GetTile(x, y);

                if (tile == null || tile.Height != 1)
                    continue;

                var region = FloodFill(map, x, y, 1, visited);

                if (region.Count < MIN_PLATFORM_SIZE)
                    continue;

                var edges = FindDepressionEdges(map, region);

                if (edges.Count < 2)
                    continue;

                PlaceDepressionRamps(map, edges);
            }
        }
    }

    List<Vector2Int> FindDepressionEdges(MapData map, List<Vector2Int> region)
    {
        var result = new List<Vector2Int>();

        foreach (var p in region)
        {
            if (!map.IsInside(p.x, p.y + 1))
                continue;

            var current = map.GetTile(p.x, p.y);
            var above = map.GetTile(p.x, p.y + 1);

            if (above == null)
                continue;

            if (above.Height > current.Height)
            {
                if (HasHorizontalSpace(map, p.x, p.y + 1))
                    result.Add(new Vector2Int(p.x, p.y + 1));
            }
        }

        return result;
    }

    bool HasHorizontalSpace(MapData map, int x, int y)
    {
        if (!map.IsInside(x - 1, y) || !map.IsInside(x + 1, y))
            return false;

        var center = map.GetTile(x, y);
        var left = map.GetTile(x - 1, y);
        var right = map.GetTile(x + 1, y);

        if (center == null || left == null || right == null)
            return false;

        if (!left.IsLand || !right.IsLand)
            return false;

        if (left.Height != center.Height)
            return false;

        if (right.Height != center.Height)
            return false;

        return true;
    }

    void PlaceDepressionRamps(MapData map, List<Vector2Int> edges)
    {
        edges.Sort((a, b) => a.x.CompareTo(b.x));

        var left = edges[0];
        var right = edges[edges.Count - 1];

        PlaceRampWithAutoDirection(map, left.x, left.y);

        if (right != left)
            PlaceRampWithAutoDirection(map, right.x, right.y);
    }

    List<Vector2Int> FloodFill(MapData map, int sx, int sy, int h, bool[,] visited)
    {
        var result = new List<Vector2Int>();
        var stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(sx, sy));

        while (stack.Count > 0)
        {
            var p = stack.Pop();

            if (!map.IsInside(p.x, p.y))
                continue;

            if (visited[p.x, p.y])
                continue;

            var t = map.GetTile(p.x, p.y);

            if (t == null || t.Height != h)
                continue;

            visited[p.x, p.y] = true;
            result.Add(p);

            stack.Push(new Vector2Int(p.x + 1, p.y));
            stack.Push(new Vector2Int(p.x - 1, p.y));
            stack.Push(new Vector2Int(p.x, p.y + 1));
            stack.Push(new Vector2Int(p.x, p.y - 1));
        }

        return result;
    }

    List<Vector2Int> FindEdgeCandidates(MapData map, List<Vector2Int> platform)
    {
        var result = new List<Vector2Int>();

        foreach (var p in platform)
        {
            if (!IsFarFromBorder(map, p.x, p.y))
                continue;

            if (!map.IsInside(p.x, p.y - 1))
                continue;

            var t = map.GetTile(p.x, p.y);
            var below = map.GetTile(p.x, p.y - 1);

            if (below == null)
                continue;

            if (t.Height > below.Height)
                result.Add(p);
        }

        return result;
    }

    void PlacePlatformRamps(MapData map, List<Vector2Int> candidates)
    {
        if (candidates.Count == 0)
            return;

        candidates.Sort((a, b) => a.x.CompareTo(b.x));

        var left = candidates[0];
        var right = candidates[candidates.Count - 1];

        PlaceRampWithAutoDirection(map, left.x, left.y);

        if (right != left)
            PlaceRampWithAutoDirection(map, right.x, right.y);
    }

    void PlaceRampWithAutoDirection(MapData map, int x, int y)
    {
        if (!map.IsInside(x, y + 1))
            return;

        var center = map.GetTile(x, y);
        var left = map.GetTile(x - 1, y);
        var right = map.GetTile(x + 1, y);

        if (center == null)
            return;

        RampDirection dir = RampDirection.East;

        if (left != null && left.Height < center.Height)
            dir = RampDirection.West;
        else if (right != null && right.Height < center.Height)
            dir = RampDirection.East;

        PlaceRamp(map, x, y, dir);
    }

    void PlaceRamp(MapData map, int x, int y, RampDirection dir)
    {
        var bottom = map.GetTile(x, y);
        var top = map.GetTile(x, y + 1);

        if (bottom == null || top == null)
            return;

        if (bottom.Type == TileType.Ramp || top.Type == TileType.Ramp)
            return;

        bottom.Type = TileType.Ramp;
        bottom.RampDirection = dir;

        top.Type = TileType.Ramp;
        top.RampDirection = dir;
    }

    bool IsFarFromBorder(MapData map, int x, int y)
    {
        return x >= BORDER_OFFSET &&
               y >= BORDER_OFFSET &&
               x < map.Width - BORDER_OFFSET &&
               y < map.Height - BORDER_OFFSET;
    }
}
