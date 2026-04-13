using UnityEngine;

public class UnitCommand
{
    private UnitCommandType _type;
    private Vector3 _position;
    private Unit _target;

    public UnitCommandType Type => _type;
    public Vector3 Position => _position;
    public Unit Target => _target;

    public static UnitCommand Move(Vector3 position)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Move,
            _position = position
        };
    }

    public static UnitCommand Attack(Unit target)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Attack,
            _target = target
        };
    }

    public static UnitCommand Hold()
    {
        return new UnitCommand
        {
            _type = UnitCommandType.HoldPosition
        };
    }

    public static UnitCommand Patrol(Vector3 a, Vector3 b)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Patrol,
            _position = a
        };
    }
}