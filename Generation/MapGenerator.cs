using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private MapDebugRenderer _debugRenderer;
    [SerializeField] private MapGenerationConfig _config;

    [Header("Terrain Strategy")]
    [SerializeField] private MonoBehaviour _terrainStrategyComponent;

    private ITerrainGenerationStrategy _terrainStrategy;

    [Header("Systems")]
    [SerializeField] private CliffDetector _cliffDetector;
    [SerializeField] private RampGenerator _rampGenerator;
    [SerializeField] private TerrainDecorationRenderer _decorationRenderer;

    private MapGenerationPipeline _pipeline;

    private void Awake()
    {
        _terrainStrategy = _terrainStrategyComponent as ITerrainGenerationStrategy;
    }

    public void Generate()
    {
        MapData map = new MapData(_config.Width, _config.Height);

        BuildPipeline();

        _pipeline.Execute(map, _config);

        _pipeline.Execute(map, _config);

        _debugRenderer.Render(map);
    }

    private void BuildPipeline()
    {
        _pipeline = new MapGenerationPipeline();

        _pipeline.AddStep(new TerrainGenerationStep(_terrainStrategy));
        _pipeline.AddStep(new HeightGenerationStep());
        _pipeline.AddStep(new CliffDetectionStep(_cliffDetector));
        _pipeline.AddStep(new RampGenerationStep(_rampGenerator));
        _pipeline.AddStep(new DecorationStep(_decorationRenderer));

    }
}