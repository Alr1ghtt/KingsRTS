public class StartPositionGenerationStep : IMapGenerationStep
{
    private readonly bool _connectBases;
    private readonly bool _forceBaseHeight;
    private readonly bool _paintCorridors;
    private readonly StartPositionGenerator _generator = new();

    public StartPositionGenerationStep(bool connectBases, bool forceBaseHeight, bool paintCorridors)
    {
        _connectBases = connectBases;
        _forceBaseHeight = forceBaseHeight;
        _paintCorridors = paintCorridors;
    }

    public void Execute(MapData map, MapGenerationConfig config)
    {
        _generator.Generate(map, config, _connectBases, _forceBaseHeight, _paintCorridors);
    }
}
