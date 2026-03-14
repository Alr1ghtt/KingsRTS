using UnityEngine;

public class ArenaGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private int _radius = 20;

    public TerrainMask Generate(int width, int height, int seed)
    {
        TerrainMask mask = new TerrainMask(width, height);

        Vector2 center = new Vector2(width / 2f, height / 2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist <= _radius)
                    mask.Set(x, y, true);
            }
        }

        return mask;
    }
}