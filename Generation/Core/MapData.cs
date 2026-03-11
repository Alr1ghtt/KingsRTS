using UnityEngine;

public class MapData
{
    public int Width { get; }
    public int Height { get; }

    private MapTile[,] _tiles;

    public MapData(int width, int height)
    {
        Width = width;
        Height = height;

        _tiles = new MapTile[width, height];
    }

    public MapTile GetTile(int x, int y)
    {
        if (!IsInside(x, y))
            return null;

        return _tiles[x, y];
    }

    public void SetTile(int x, int y, MapTile tile)
    {
        if (!IsInside(x, y))
            return;

        _tiles[x, y] = tile;
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public MapTile[,] GetRawArray()
    {
        return _tiles;
    }
}