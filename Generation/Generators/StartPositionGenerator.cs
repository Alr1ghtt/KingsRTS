using System.Collections.Generic;
using UnityEngine;

public class StartPositionGenerator
{
    private const int WaterPathPenalty = 18;
    private const int EdgePenalty = 30;
    private const float PathNoiseScale = 0.09f;

    public void Generate(MapData map, MapGenerationConfig config, bool connectBases, bool forceBaseHeight, bool paintCorridors)
    {
        List<Vector2Int> positions = GetStartPositions(map, config);

        foreach (Vector2Int position in positions)
            CreateBase(map, position.x, position.y, config.BaseRadius, forceBaseHeight);

        if (!connectBases || !paintCorridors)
            return;

        ConnectBases(map, config, positions);
    }

    private List<Vector2Int> GetStartPositions(MapData map, MapGenerationConfig config)
    {
        int count = Mathf.Max(1, config.PlayerCount);
        int radius = config.BaseRadius;
        int margin = radius + config.BasePlacementMargin;

        float cx = map.Width / 2f;
        float cy = map.Height / 2f;
        float rx = map.Width / 2f - margin;
        float ry = map.Height / 2f - margin;
        float step = 360f / count;

        var result = new List<Vector2Int>();

        for (int i = 0; i < count; i++)
        {
            float angle = step * i * Mathf.Deg2Rad;

            int x = Mathf.RoundToInt(cx + Mathf.Cos(angle) * rx);
            int y = Mathf.RoundToInt(cy + Mathf.Sin(angle) * ry);

            x = Mathf.Clamp(x, margin, map.Width - margin - 1);
            y = Mathf.Clamp(y, margin, map.Height - margin - 1);

            result.Add(new Vector2Int(x, y));
        }

        return result;
    }

    private void ConnectBases(MapData map, MapGenerationConfig config, List<Vector2Int> positions)
    {
        if (positions.Count == 0)
            return;

        int width = Mathf.Max(3, config.BaseConnectionWidth);

        foreach (Vector2Int position in positions)
        {
            Vector2Int anchor = FindNearestExternalLand(map, position, config.BaseRadius + width + 2);

            if (anchor != position)
                CreateOrganicCorridor(map, config, position, anchor, width);
        }

        for (int i = 1; i < positions.Count; i++)
        {
            if (AreConnected(map, positions[0], positions[i]))
                continue;

            CreateOrganicCorridor(map, config, positions[0], positions[i], width);
        }

        if (positions.Count > 2)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2Int current = positions[i];
                Vector2Int next = positions[(i + 1) % positions.Count];

                if (AreConnected(map, current, next))
                    continue;

                CreateOrganicCorridor(map, config, current, next, width);
            }
        }
    }

    private Vector2Int FindNearestExternalLand(MapData map, Vector2Int from, int excludeRadius)
    {
        Vector2Int best = from;
        int bestDistance = int.MaxValue;
        int excludeSqr = excludeRadius * excludeRadius;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (!IsLand(map, x, y))
                    continue;

                int dx = x - from.x;
                int dy = y - from.y;
                int distance = dx * dx + dy * dy;

                if (distance <= excludeSqr)
                    continue;

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = new Vector2Int(x, y);
            }
        }

        return best;
    }

    private void CreateOrganicCorridor(MapData map, MapGenerationConfig config, Vector2Int from, Vector2Int to, int width)
    {
        List<Vector2Int> path = FindPath(map, config, from, to);

        if (path == null || path.Count == 0)
        {
            PaintCurvedFallback(map, config, from, to, width);
            return;
        }

        int radius = Mathf.Max(1, width / 2);

        foreach (Vector2Int point in path)
            PaintLandCircle(map, point.x, point.y, radius, false);
    }

    private List<Vector2Int> FindPath(MapData map, MapGenerationConfig config, Vector2Int from, Vector2Int to)
    {
        float[,] cost = new float[map.Width, map.Height];
        bool[,] closed = new bool[map.Width, map.Height];
        Vector2Int[,] parent = new Vector2Int[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                cost[x, y] = float.MaxValue;
                parent[x, y] = new Vector2Int(-1, -1);
            }
        }

        List<Vector2Int> open = new List<Vector2Int>();
        cost[from.x, from.y] = 0f;
        open.Add(from);

        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        int guard = map.Width * map.Height * 8;

        while (open.Count > 0 && guard > 0)
        {
            int currentIndex = GetBestOpenIndex(open, cost, to);
            Vector2Int current = open[currentIndex];
            open.RemoveAt(currentIndex);

            if (closed[current.x, current.y])
                continue;

            closed[current.x, current.y] = true;

            if (current == to)
                return BuildPath(parent, from, to);

            foreach (Vector2Int direction in directions)
            {
                int nx = current.x + direction.x;
                int ny = current.y + direction.y;

                if (!map.IsInside(nx, ny))
                    continue;

                if (closed[nx, ny])
                    continue;

                Vector2Int next = new Vector2Int(nx, ny);
                float stepCost = GetStepCost(map, config, next, direction);
                float nextCost = cost[current.x, current.y] + stepCost;

                if (nextCost >= cost[nx, ny])
                    continue;

                cost[nx, ny] = nextCost;
                parent[nx, ny] = current;
                open.Add(next);
            }

            guard--;
        }

        return null;
    }

    private int GetBestOpenIndex(List<Vector2Int> open, float[,] cost, Vector2Int target)
    {
        int bestIndex = 0;
        float bestScore = float.MaxValue;

        for (int i = 0; i < open.Count; i++)
        {
            Vector2Int point = open[i];
            float heuristic = Vector2Int.Distance(point, target);
            float score = cost[point.x, point.y] + heuristic;

            if (score >= bestScore)
                continue;

            bestScore = score;
            bestIndex = i;
        }

        return bestIndex;
    }

    private float GetStepCost(MapData map, MapGenerationConfig config, Vector2Int point, Vector2Int direction)
    {
        float diagonal = direction.x != 0 && direction.y != 0 ? 1.35f : 1f;
        float landCost = IsLand(map, point.x, point.y) ? 1f : WaterPathPenalty;
        float edgeCost = GetEdgeCost(map, point);
        float noise = Mathf.PerlinNoise(
            (point.x + config.Seed * 17) * PathNoiseScale,
            (point.y + config.Seed * 31) * PathNoiseScale
        );

        return diagonal * landCost + edgeCost + noise * 8f;
    }

    private float GetEdgeCost(MapData map, Vector2Int point)
    {
        int margin = 4;

        if (point.x < margin || point.y < margin || point.x >= map.Width - margin || point.y >= map.Height - margin)
            return EdgePenalty;

        return 0f;
    }

    private List<Vector2Int> BuildPath(Vector2Int[,] parent, Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = to;
        int guard = parent.GetLength(0) * parent.GetLength(1);

        while (current != from && guard > 0)
        {
            path.Add(current);
            current = parent[current.x, current.y];

            if (current.x < 0 || current.y < 0)
                return null;

            guard--;
        }

        path.Add(from);
        path.Reverse();

        return path;
    }

    private void PaintCurvedFallback(MapData map, MapGenerationConfig config, Vector2Int from, Vector2Int to, int width)
    {
        Vector2 direction = ((Vector2)(to - from)).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x);
        float distance = Vector2.Distance(from, to);
        float amplitude = Mathf.Clamp(distance * 0.18f, 4f, 18f);

        int steps = Mathf.CeilToInt(distance);
        int radius = Mathf.Max(1, width / 2);

        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0f : i / (float)steps;
            float wave = Mathf.Sin(t * Mathf.PI) * amplitude;
            float noise = Mathf.PerlinNoise(
                (from.x + i + config.Seed * 11) * 0.07f,
                (from.y + i + config.Seed * 23) * 0.07f
            );

            Vector2 point = Vector2.Lerp(from, to, t) + normal * wave * (noise > 0.5f ? 1f : -1f);
            Vector2Int gridPoint = Vector2Int.RoundToInt(point);

            PaintLandCircle(map, gridPoint.x, gridPoint.y, radius, false);
        }
    }

    private void CreateBase(MapData map, int cx, int cy, int radius, bool forceHeight)
    {
        PaintLandCircle(map, cx, cy, radius, forceHeight);
    }

    private void PaintLandCircle(MapData map, int cx, int cy, int radius, bool forceHeight)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (!map.IsInside(x, y))
                    continue;

                if (!CircleUtility.InsideCircle(x, y, cx, cy, radius))
                    continue;

                MakeLand(map, x, y, forceHeight);
            }
        }
    }

    private void MakeLand(MapData map, int x, int y, bool forceHeight)
    {
        MapTile tile = map.GetTile(x, y);

        if (tile == null)
        {
            tile = new MapTile(new Vector2Int(x, y), true);
            map.SetTile(x, y, tile);
        }

        tile.IsLand = true;
        tile.Type = TileType.Terrain;
        tile.RampDirection = RampDirection.None;

        if (forceHeight)
            tile.Height = 1;
        else if (tile.Height <= 0)
            tile.Height = 1;
    }

    private bool AreConnected(MapData map, Vector2Int start, Vector2Int target)
    {
        if (!IsWalkable(map, start.x, start.y) || !IsWalkable(map, target.x, target.y))
            return false;

        bool[,] visited = new bool[map.Width, map.Height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        visited[start.x, start.y] = true;
        queue.Enqueue(start);

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == target)
                return true;

            foreach (Vector2Int direction in directions)
            {
                int nx = current.x + direction.x;
                int ny = current.y + direction.y;

                if (!map.IsInside(nx, ny))
                    continue;

                if (visited[nx, ny])
                    continue;

                if (!IsWalkable(map, nx, ny))
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return false;
    }

    private bool IsWalkable(MapData map, int x, int y)
    {
        MapTile tile = map.GetTile(x, y);

        if (tile == null)
            return false;

        return tile.IsLand && tile.Type != TileType.Cliff;
    }

    private bool IsLand(MapData map, int x, int y)
    {
        MapTile tile = map.GetTile(x, y);

        return tile != null && tile.IsLand;
    }
}
