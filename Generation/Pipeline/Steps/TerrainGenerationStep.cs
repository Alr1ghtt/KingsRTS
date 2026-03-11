using UnityEngine;

public class TerrainGenerationStep : IMapGenerationStep
{
    private readonly ITerrainGenerationStrategy _strategy;

    public TerrainGenerationStep(ITerrainGenerationStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Execute(MapData map, MapGenerationConfig config)
    {
        TerrainMask mask = _strategy.Generate(
            map.Width,
            map.Height,
            config.Seed
        );

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                bool isLand = mask.Get(x, y);

                MapTile tile = new MapTile(
                    new Vector2Int(x, y),
                    isLand
                );

                map.SetTile(x, y, tile);
            }
        }
    }
}