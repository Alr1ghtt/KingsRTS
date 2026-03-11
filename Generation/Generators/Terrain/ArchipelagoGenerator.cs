using UnityEngine;

public class ArchipelagoGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private int _islandCount = 20;
    [SerializeField] private int _radius = 6;

    public TerrainMask Generate(int width, int height, int seed)
    {
        Random.InitState(seed);

        TerrainMask mask = new TerrainMask(width, height);

        for (int i = 0; i < _islandCount; i++)
        {
            int cx = Random.Range(0, width);
            int cy = Random.Range(0, height);

            for (int x = -_radius; x <= _radius; x++)
            {
                for (int y = -_radius; y <= _radius; y++)
                {
                    int px = cx + x;
                    int py = cy + y;

                    if (px < 0 || py < 0 || px >= width || py >= height)
                        continue;

                    float dist = Mathf.Sqrt(x * x + y * y);

                    if (dist <= _radius)
                        mask.Set(px, py, true);
                }
            }
        }

        return mask;
    }
}