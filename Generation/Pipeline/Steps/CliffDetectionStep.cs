public class CliffDetectionStep : IMapGenerationStep
{
    private readonly CliffDetector _detector;

    public CliffDetectionStep(CliffDetector detector)
    {
        _detector = detector;
    }

    public void Execute(MapData map, MapGenerationConfig config)
    {
        _detector.Detect(map);
    }
}