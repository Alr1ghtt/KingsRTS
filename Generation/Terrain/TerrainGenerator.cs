using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public void Generate(MapData data, MapGenerationConfig config)
    {
        Random.InitState(config.Seed);

        for (int x = 0; x < data.Width; x++)
        {
            for (int y = 0; y < data.Height; y++)
            {
                bool isLand = Random.value > 0.3f;

                MapTile tile = new MapTile(new Vector2Int(x, y), isLand);

                if (isLand)
                {
                    tile.Height = Random.Range(0, config.Levels);
                }

                data.SetTile(x, y, tile);
            }
        }
    }
}