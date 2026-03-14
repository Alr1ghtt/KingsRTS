using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private TerrainTilemapRenderer _renderer;
    [SerializeField] private MapGenerationConfig _config;

    [Header("Terrain Strategy")]
    [SerializeField] private MonoBehaviour _terrainStrategyComponent;

    private ITerrainGenerationStrategy _terrainStrategy;

    [Header("Systems")]
    [SerializeField] private CliffDetector _cliffDetector;
    [SerializeField] private RampGenerator _rampGenerator;

    [SerializeField] private TilemapLayerSystem _layers;

    private MapGenerationPipeline _pipeline;

    public void Generate()
    {
        if (_terrainStrategyComponent is ITerrainGenerationStrategy strategy)
            _terrainStrategy = strategy;
        else
        {
            Debug.LogError("Terrain strategy not initialized");
            return;
        }

        if (_layers != null)
            _layers.ClearAll();

        MapData map = new MapData(_config.Width, _config.Height);

        BuildPipeline();

        _pipeline.Execute(map, _config);

        _renderer.Render(map);
    }

    private void BuildPipeline()
    {
        _pipeline = new MapGenerationPipeline();

        _pipeline.AddStep(new TerrainGenerationStep(_terrainStrategy));
        _pipeline.AddStep(new HeightGenerationStep());
        _pipeline.AddStep(new CliffDetectionStep(_cliffDetector));
        _pipeline.AddStep(new RampGenerationStep(_rampGenerator));
    }
}