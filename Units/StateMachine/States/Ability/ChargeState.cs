using UnityEngine;

public class ChargeState : IUnitState
{
    public void Enter(UnitContext context) { }

    public void Update(UnitContext context, float deltaTime)
    {
        if (context.TargetUnit == null)
            return;

        var dir = (context.TargetUnit.Context.Transform.position - context.Transform.position).normalized;
        context.Transform.position += dir * context.Data.MoveSpeed * 2f * deltaTime;
    }

    public void Exit(UnitContext context) { }
}