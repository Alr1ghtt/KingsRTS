using UnityEngine;

public static class RampPlacementValidator
{
    public static bool CanPlace(MapData map, int x, int y)
    {
        if (!map.IsInside(x, y) || !map.IsInside(x, y + 1))
            return false;

        var bottom = map.GetTile(x, y);
        var top = map.GetTile(x, y + 1);

        if (bottom == null || top == null)
            return false;

        if (!bottom.IsLand || !top.IsLand)
            return false;

        if (top.Height - bottom.Height != 1)
            return false;

        // КЛЮЧЕВОЕ: проверка "площадки"
        if (!HasFlatTop(map, x, y + 1))
            return false;

        if (!HasFlatBottom(map, x, y))
            return false;

        // КЛЮЧЕВОЕ: проверка направления (только горизонталь)
        bool validHorizontal =
            IsSideLower(map, x - 1, y, top.Height) ||
            IsSideLower(map, x + 1, y, top.Height);

        if (!validHorizontal)
            return false;

        // ЗАПРЕТ вертикальных рамп
        if (IsSideLower(map, x, y + 2, top.Height))
            return false;

        if (IsSideLower(map, x, y - 1, bottom.Height))
            return false;

        return true;
    }

    static bool HasFlatTop(MapData map, int x, int y)
    {
        int h = map.GetTile(x, y).Height;

        return EqualHeight(map, x - 1, y, h) &&
               EqualHeight(map, x, y, h) &&
               EqualHeight(map, x + 1, y, h);
    }

    static bool HasFlatBottom(MapData map, int x, int y)
    {
        int h = map.GetTile(x, y).Height;

        return EqualHeight(map, x - 1, y, h) &&
               EqualHeight(map, x, y, h) &&
               EqualHeight(map, x + 1, y, h);
    }

    static bool EqualHeight(MapData map, int x, int y, int h)
    {
        if (!map.IsInside(x, y))
            return false;

        var t = map.GetTile(x, y);
        return t != null && t.Height == h && t.IsLand;
    }

    static bool IsSideLower(MapData map, int x, int y, int h)
    {
        if (!map.IsInside(x, y))
            return false;

        var t = map.GetTile(x, y);
        if (t == null) return false;

        return t.Height < h;
    }
}