using UnityEngine;

public class ArchipelagoGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private int _islandCount = 12;
    [SerializeField] private int _minRadius = 10;
    [SerializeField] private int _maxRadius = 20;
    [SerializeField] private float _noiseScale = 0.08f;
    [SerializeField] private int _smoothIterations = 2;

    public TerrainMask Generate(int width, int height, int seed)
    {
        Random.InitState(seed);

        TerrainMask mask = new TerrainMask(width, height);

        for (int i = 0; i < _islandCount; i++)
        {
            int radius = Random.Range(_minRadius, _maxRadius + 1);
            int cx = Random.Range(radius, width - radius);
            int cy = Random.Range(radius, height - radius);

            CreateIsland(mask, cx, cy, radius, seed);
        }

        for (int i = 0; i < _smoothIterations; i++)
            Smooth(mask);

        return mask;
    }

    private void CreateIsland(TerrainMask mask, int cx, int cy, int radius, int seed)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (x < 0 || y < 0 || x >= mask.Width || y >= mask.Height)
                    continue;

                float dx = x - cx;
                float dy = y - cy;
                float distance = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                float noise = Mathf.PerlinNoise((x + seed) * _noiseScale, (y + seed) * _noiseScale);
                float edge = 0.72f + noise * 0.35f;

                if (distance <= edge)
                    mask.Set(x, y, true);
            }
        }
    }

    private void Smooth(TerrainMask mask)
    {
        bool[,] result = new bool[mask.Width, mask.Height];

        for (int x = 0; x < mask.Width; x++)
        {
            for (int y = 0; y < mask.Height; y++)
            {
                int count = CountLand(mask, x, y);
                bool land = mask.Get(x, y);

                result[x, y] = count >= 5 || land && count >= 3;
            }
        }

        for (int x = 0; x < mask.Width; x++)
            for (int y = 0; y < mask.Height; y++)
                mask.Set(x, y, result[x, y]);
    }

    private int CountLand(TerrainMask mask, int x, int y)
    {
        int count = 0;

        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                if (ox == 0 && oy == 0)
                    continue;

                int px = x + ox;
                int py = y + oy;

                if (px < 0 || py < 0 || px >= mask.Width || py >= mask.Height)
                    continue;

                if (mask.Get(px, py))
                    count++;
            }
        }

        return count;
    }
}
