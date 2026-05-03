using UnityEngine;

public class NoiseTerrainGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private float _scale = 0.06f;
    [SerializeField] private float _threshold = 0.43f;
    [SerializeField] private int _smoothIterations = 2;

    public TerrainMask Generate(int width, int height, int seed)
    {
        TerrainMask mask = new TerrainMask(width, height);
        float offsetX = seed * 0.113f;
        float offsetY = seed * 0.271f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noise = Mathf.PerlinNoise((x + offsetX) * _scale, (y + offsetY) * _scale);

                if (noise > _threshold)
                    mask.Set(x, y, true);
            }
        }

        for (int i = 0; i < _smoothIterations; i++)
            Smooth(mask);

        return mask;
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
