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
        var leftEdges = new List<Vector2Int>();
        var rightEdges = new List<Vector2Int>();

        foreach (var p in region)
        {
            int x = p.x;
            int y = p.y;

            var current = map.GetTile(x, y);
            if (current == null)
                continue;

            // LEFT wall (ставим рампу ВНУТРИ впадины)
            if (map.IsInside(x - 1, y))
            {
                var left1 = map.GetTile(x - 1, y);

                if (left1 != null && left1.Height > current.Height)
                {
                    if (IsValidRampSpot(map, x, y))
                        leftEdges.Add(new Vector2Int(x, y));
                }
            }

            // RIGHT wall
            if (map.IsInside(x + 1, y))
            {
                var right1 = map.GetTile(x + 1, y);

                if (right1 != null && right1.Height > current.Height)
                {
                    if (IsValidRampSpot(map, x, y))
                        rightEdges.Add(new Vector2Int(x, y));
                }
            }
        }

        var result = new List<Vector2Int>();

        var left = GetSpaced(leftEdges);
        var right = GetSpaced(rightEdges);

        if (left.HasValue)
            result.Add(left.Value);

        if (right.HasValue)
            result.Add(right.Value);

        return result;
    }
    Vector2Int? GetSpaced(List<Vector2Int> list)
    {
        if (list.Count == 0)
            return null;

        list.Sort((a, b) => a.y.CompareTo(b.y));

        var mid = list[list.Count / 2];

        // проверка дистанции от других рамп (2 клетки)
        foreach (var p in list)
        {
            if (Mathf.Abs(p.y - mid.y) >= 2)
                return p;
        }

        return mid;
    }
    bool IsValidRampSpot(MapData map, int x, int y)
    {
        // нельзя в углах
        if (!map.IsInside(x, y + 1))
            return false;

        var top = map.GetTile(x, y + 1);
        var center = map.GetTile(x, y);

        if (top == null || center == null)
            return false;

        // должен быть перепад вверх
        if (top.Height <= center.Height)
            return false;

        // слева и справа должны быть "ровные" стены
        var l = map.GetTile(x - 1, y);
        var r = map.GetTile(x + 1, y);

        if (l == null || r == null)
            return false;

        if (l.Height == center.Height && r.Height == center.Height)
            return false; // это не край

        return true;
    }

    Vector2Int GetMiddle(List<Vector2Int> list)
    {
        list.Sort((a, b) => a.y.CompareTo(b.y));
        return list[list.Count / 2];
    }

    void PlaceDepressionRamps(MapData map, List<Vector2Int> edges)
    {
        if (edges.Count == 0)
            return;

        edges.Sort((a, b) => a.x.CompareTo(b.x));

        var left = edges[0];
        var right = edges[edges.Count - 1];

        PlaceRamp(map, left.x, left.y, RampDirection.East);

        if (right != left)
            PlaceRamp(map, right.x, right.y, RampDirection.West);
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
