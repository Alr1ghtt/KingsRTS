using UnityEngine;

public class PlateauGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    public TerrainMask Generate(int width, int height, int seed)
    {
        TerrainMask mask = new TerrainMask(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mask.Set(x, y, true);
            }
        }

        return mask;
    }
}