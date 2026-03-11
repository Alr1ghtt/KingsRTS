public class DecorationStep : IMapGenerationStep
{
    private readonly TerrainDecorationRenderer _renderer;

    public DecorationStep(TerrainDecorationRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Execute(MapData map, MapGenerationConfig config)
    {
        _renderer.Render(map);
    }
}