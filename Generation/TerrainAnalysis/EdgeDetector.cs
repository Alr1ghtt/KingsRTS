public enum EdgeType
{
    None,
    Top,
    Bottom,
    Left,
    Right,
    Corner
}

public class EdgeDetector
{
    public EdgeType Detect(TerrainMask mask, int x, int y)
    {
        bool up = IsSolid(mask, x, y + 1);
        bool down = IsSolid(mask, x, y - 1);
        bool left = IsSolid(mask, x - 1, y);
        bool right = IsSolid(mask, x + 1, y);

        if (!down) return EdgeType.Bottom;
        if (!up) return EdgeType.Top;
        if (!left) return EdgeType.Left;
        if (!right) return EdgeType.Right;

        return EdgeType.None;
    }

    private bool IsSolid(TerrainMask mask, int x, int y)
    {
        if (x < 0 || y < 0 || x >= mask.Width || y >= mask.Height)
            return false;

        return mask.Get(x, y);
    }
}