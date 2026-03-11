public class RampPlacementValidator
{
    public bool CanPlaceRamp(TerrainMask mask, int x, int y)
    {
        if (x < 1 || x >= mask.Width - 2)
            return false;

        bool a = mask.Get(x - 1, y);
        bool b = mask.Get(x, y);
        bool c = mask.Get(x + 1, y);

        if (a && b && c)
            return true;

        return false;
    }
}