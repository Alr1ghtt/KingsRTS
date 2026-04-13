using UnityEngine;

public class HealSingleState : IUnitState
{
    public void Enter(UnitContext context) { }

    public void Update(UnitContext context, float deltaTime)
    {
        if (context.TargetUnit == null)
            return;

        if (context.AttackCooldown > 0f)
            return;

        context.TargetUnit.Context.CurrentHealth += 10f;
        context.AttackCooldown = 1.5f;
    }

    public void Exit(UnitContext context) { }
}