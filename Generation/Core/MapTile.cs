using UnityEngine;

public class MapTile
{
    public Vector2Int Position { get; }

    public bool IsLand { get; set; }

    public int Height { get; set; }

    public TileType Type { get; set; }

    public RampDirection RampDirection { get; set; }

    public MapTile(Vector2Int position, bool isLand)
    {
        Position = position;
        IsLand = isLand;
        Height = isLand ? 1 : 0;
        Type = isLand ? TileType.Terrain : TileType.Empty;
        RampDirection = RampDirection.None;
    }
}
