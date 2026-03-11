using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public TerrainMask GenerateBaseLevel(int width, int height)
    {
        TerrainMask mask = new TerrainMask(width, height);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                mask.Set(x, y, true);
            }

        return mask;
    }

    public TerrainMask GenerateNextLevel(TerrainMask previous, float noiseScale, float threshold)
    {
        int width = previous.Width;
        int height = previous.Height;

        TerrainMask mask = new TerrainMask(width, height);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!previous.Get(x, y))
                    continue;

                float noise = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                if (noise > threshold)
                    mask.Set(x, y, true);
            }

        return mask;
    }
}