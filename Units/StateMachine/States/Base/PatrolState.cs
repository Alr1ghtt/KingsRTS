using UnityEngine;

public class PatrolState : IUnitState
{
    private Vector3 _pointA;
    private Vector3 _pointB;
    private bool _toB;

    public PatrolState(Vector3 pointA, Vector3 pointB)
    {
        _pointA = pointA;
        _pointB = pointB;
    }

    public void Enter(UnitContext context)
    {
        _toB = true;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        var target = _toB ? _pointB : _pointA;
        var dir = (target - context.Transform.position);

        if (dir.sqrMagnitude < 0.1f)
        {
            _toB = !_toB;
            return;
        }

        dir.Normalize();
        context.Transform.position += dir * context.Data.MoveSpeed * deltaTime;
    }

    public void Exit(UnitContext context) { }
}