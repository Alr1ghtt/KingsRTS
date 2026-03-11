public class RampGenerationStep : IMapGenerationStep
{
    private readonly RampGenerator _generator;

    public RampGenerationStep(RampGenerator generator)
    {
        _generator = generator;
    }

    public void Execute(MapData map, MapGenerationConfig config)
    {
        _generator.Generate(map);
    }
}