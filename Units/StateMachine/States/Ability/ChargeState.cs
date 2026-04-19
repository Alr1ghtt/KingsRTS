using UnityEngine;

public class ChargeState : IUnitState
{
    public void Enter(UnitContext context)
    {
    }

    public void Update(UnitContext context, float deltaTime)
    {
        var targetUnit = context.AttackTarget as Unit;
        if (targetUnit == null)
            return;

        if (!targetUnit.IsAlive)
            return;

        var direction = (targetUnit.Context.Transform.position - context.Transform.position).normalized;
        context.Transform.position += direction * context.Data.MoveSpeed * 2f * deltaTime;
    }

    public void Exit(UnitContext context)
    {
    }
}