using UnityEngine;

public class HeightLevelGenerator
{
    private readonly int _maxHeight;
    private readonly float _scale;
    private readonly int _seed;

    public HeightLevelGenerator(int maxHeight, float scale, int seed)
    {
        _maxHeight = maxHeight;
        _scale = scale;
        _seed = seed;
    }

    public void Generate(MapData map)
    {
        float offsetX = _seed * 0.123f;
        float offsetY = _seed * 0.456f;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(x, y);

                if (!tile.IsLand)
                {
                    tile.Height = 0;
                    continue;
                }

                float nx = (x + offsetX) * _scale;
                float ny = (y + offsetY) * _scale;

                float noise = Mathf.PerlinNoise(nx, ny);

                tile.Height = Mathf.Clamp(
                Mathf.FloorToInt(noise * _maxHeight) + 1,
                0,
                _maxHeight
                );
            }
        }
    }
}