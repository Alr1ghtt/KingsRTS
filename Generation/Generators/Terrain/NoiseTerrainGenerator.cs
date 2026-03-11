using UnityEngine;

public class NoiseTerrainGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private float _scale = 0.08f;
    [SerializeField] private float _threshold = 0.45f;

    public TerrainMask Generate(int width, int height, int seed)
    {
        Random.InitState(seed);

        TerrainMask mask = new TerrainMask(width, height);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float noise = Mathf.PerlinNoise(x * _scale, y * _scale);

                if (noise > _threshold)
                    mask.Set(x, y, true);
            }

        return mask;
    }
}