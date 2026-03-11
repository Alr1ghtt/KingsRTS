public class TerrainMask
{
    private bool[,] _mask;

    public int Width { get; }
    public int Height { get; }

    public TerrainMask(int width, int height)
    {
        Width = width;
        Height = height;
        _mask = new bool[width, height];
    }

    public bool Get(int x, int y)
    {
        return _mask[x, y];
    }

    public void Set(int x, int y, bool value)
    {
        _mask[x, y] = value;
    }
}