public class TileResolver
{
    public int GetBitmask(TerrainMask mask, int x, int y)
    {
        int maskValue = 0;

        if (IsSolid(mask, x, y + 1)) maskValue |= 1;
        if (IsSolid(mask, x + 1, y)) maskValue |= 2;
        if (IsSolid(mask, x, y - 1)) maskValue |= 4;
        if (IsSolid(mask, x - 1, y)) maskValue |= 8;

        return maskValue;
    }

    private bool IsSolid(TerrainMask mask, int x, int y)
    {
        if (x < 0 || y < 0 || x >= mask.Width || y >= mask.Height)
            return false;

        return mask.Get(x, y);
    }
}