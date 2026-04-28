using UnityEngine;

public class UnitCombatSystem
{
    private const float AttackWindup = 0.25f;

    public bool TryAttack(UnitContext context, IAttackTarget target)
    {
        if (!context.Owner.CanAttack)
            return false;

        if (target == null)
            return false;

        if (!target.IsAlive)
            return false;

        if (context.AttackCooldown > 0f)
            return false;

        if (context.IsAttackAnimationLocked)
            return false;

        context.IsMoving = false;
        context.CurrentVelocity = Vector3.zero;
        context.MoveDirection = Vector3.zero;
        context.PendingAttackTarget = target;
        context.AttackWindupTimer = AttackWindup;
        context.IsAttackAnimationLocked = true;
        context.Owner.PlayAttackAnimation(target);
        return true;
    }

    public void UpdateAttack(UnitContext context, UnitTargetingSystem targetingSystem)
    {
        if (!context.IsAttackAnimationLocked)
            return;

        if (context.AttackWindupTimer > 0f)
            return;

        var target = context.PendingAttackTarget;

        if (target != null && targetingSystem.IsInAttackRange(context, target))
            target.TakeDamage(context.Data.AttackDamage);

        context.PendingAttackTarget = null;
        context.IsAttackAnimationLocked = false;
        context.AttackCooldown = context.Data.AttackSpeed > 0f ? 1f / context.Data.AttackSpeed : 0f;
        context.Owner.ForceRefreshAnimation();
    }

    public void CancelAttack(UnitContext context)
    {
        context.PendingAttackTarget = null;
        context.AttackWindupTimer = 0f;
        context.IsAttackAnimationLocked = false;
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