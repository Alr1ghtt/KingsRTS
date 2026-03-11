using System.Collections.Generic;

public static class FloodFillUtility
{
    public static void Fill(bool[,] map, int startX, int startY)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Queue<(int x, int y)> queue = new();

        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            if (x < 0 || y < 0 || x >= width || y >= height)
                continue;

            if (map[x, y])
                continue;

            map[x, y] = true;

            queue.Enqueue((x + 1, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x, y + 1));
            queue.Enqueue((x, y - 1));
        }
    }
}