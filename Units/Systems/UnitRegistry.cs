using System.Collections.Generic;

public static class UnitRegistry
{
    private static readonly List<Unit> _units = new();

    public static IReadOnlyList<Unit> Units => _units;

    public static void Register(Unit unit)
    {
        if (unit == null)
            return;

        if (_units.Contains(unit))
            return;

        _units.Add(unit);
    }

    public static void Unregister(Unit unit)
    {
        if (unit == null)
            return;

        _units.Remove(unit);
    }
}