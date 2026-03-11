public static class CircleUtility
{
    public static bool InsideCircle(int x, int y, int cx, int cy, int radius)
    {
        int dx = x - cx;
        int dy = y - cy;

        return dx * dx + dy * dy <= radius * radius;
    }
}