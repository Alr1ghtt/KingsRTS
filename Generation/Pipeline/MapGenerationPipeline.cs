using System.Collections.Generic;

public class MapGenerationPipeline
{
    private readonly List<IMapGenerationStep> _steps = new();

    public void AddStep(IMapGenerationStep step)
    {
        _steps.Add(step);
    }

    public void Execute(MapData map, MapGenerationConfig config)
    {
        foreach (var step in _steps)
        {
            step.Execute(map, config);
        }
    }
}