using UnityEngine;

public class MoveState : IUnitState
{
    public void Enter(UnitContext context) { }

    public void Update(UnitContext context, float deltaTime)
    {
        var direction = (context.TargetPosition - context.Transform.position);
        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();
        context.Transform.position += direction * context.Data.MoveSpeed * deltaTime;
    }

    public void Exit(UnitContext context) { }
}