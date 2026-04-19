using UnityEngine;

public class HealSingleState : IUnitState
{
    public void Enter(UnitContext context)
    {
    }

    public void Update(UnitContext context, float deltaTime)
    {
        var targetUnit = context.RepairTarget as Unit;
        if (targetUnit == null)
            return;

        if (!targetUnit.IsAlive)
            return;

        if (context.AttackCooldown > 0f)
            return;

        targetUnit.Repair(10f);
        context.AttackCooldown = 1.5f;
    }

    public void Exit(UnitContext context)
    {
    }
}