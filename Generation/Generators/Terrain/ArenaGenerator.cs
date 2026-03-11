using UnityEngine;

public class ArenaGenerator : MonoBehaviour, ITerrainGenerationStrategy
{
    [SerializeField] private int _radius = 20;

    public TerrainMask Generate(int width, int height, int seed)
    {
        TerrainMask mask = new TerrainMask(width, height);

        int cx = width / 2;
        int cy = height / 2;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                if (dist < _radius)
                    mask.Set(x, y, true);
            }

        return mask;
    }
}