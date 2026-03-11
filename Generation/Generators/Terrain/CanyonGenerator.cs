using UnityEngine;

public class CanyonGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private float _scale = 0.06f;

    public TerrainMask Generate(int width, int height, int seed)
    {
        Random.InitState(seed);

        TerrainMask mask = new TerrainMask(width, height);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float noise = Mathf.PerlinNoise(x * _scale, y * _scale);

                if (noise > 0.55f)
                    mask.Set(x, y, true);
            }

        for (int x = width / 3; x < width / 3 + 3; x++)
            for (int y = 0; y < height; y++)
                mask.Set(x, y, false);

        int corridorX = width / 2;

        for (int y = 0; y < height; y++)
        {
            mask.Set(corridorX, y, true);
        }
        return mask;
    }
}