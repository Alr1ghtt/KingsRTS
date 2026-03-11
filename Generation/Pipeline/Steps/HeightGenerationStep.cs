public class HeightGenerationStep : IMapGenerationStep
{
    public void Execute(MapData map, MapGenerationConfig config)
    {
        var generator = new HeightLevelGenerator(
            config.Levels,
            config.HeightNoiseScale,
            config.Seed
        );

        generator.Generate(map);
    }
}