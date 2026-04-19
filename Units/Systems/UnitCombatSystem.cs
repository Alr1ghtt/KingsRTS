using UnityEngine;

public class UnitCombatSystem
{
    public bool TryAttack(UnitContext context, IAttackTarget target)
    {
        if (!context.Data.CanAttack)
            return false;

        if (target == null)
            return false;

        if (!target.IsAlive)
            return false;

        if (context.AttackCooldown > 0f)
            return false;

        target.TakeDamage(context.Data.AttackDamage);
        context.AttackCooldown = context.Data.AttackSpeed > 0f ? 1f / context.Data.AttackSpeed : 0f;
        return true;
    }

    public void Repair(UnitContext context, IRepairTarget target, float deltaTime)
    {
        if (!context.Data.CanRepair)
            return;

        if (target == null)
            return;

        if (!target.IsAlive)
            return;

        if (!target.CanBeRepaired)
            return;

        if (!target.NeedsRepair)
            return;

        target.Repair(context.Data.RepairRate * deltaTime);
    }
}