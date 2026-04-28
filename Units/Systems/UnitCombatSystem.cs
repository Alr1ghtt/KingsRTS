using UnityEngine;

public class UnitCombatSystem
{
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
        context.AttackStartPosition = context.Transform.position;
        context.AttackTargetPosition = target.Position;
        context.AttackDamage = context.Data.AttackDamage;
        context.AttackTimer = 0f;
        context.AttackHitApplied = false;
        context.IsAttackAnimationLocked = true;

        var profile = context.Owner.PlayAttackAnimation(target);
        context.AttackDuration = Mathf.Max(0.01f, profile.Duration);
        context.AttackHitTime = Mathf.Clamp(profile.HitTime, 0f, context.AttackDuration);
        return true;
    }

    public void UpdateAttack(UnitContext context, UnitTargetingSystem targetingSystem, float deltaTime)
    {
        if (!context.IsAttackAnimationLocked)
            return;

        context.IsMoving = false;
        context.CurrentVelocity = Vector3.zero;
        context.MoveDirection = Vector3.zero;
        context.AttackTimer += deltaTime;

        if (!context.AttackHitApplied && context.AttackTimer >= context.AttackHitTime)
        {
            ApplyAttackHit(context, targetingSystem);
            context.AttackHitApplied = true;
        }

        if (context.AttackTimer < context.AttackDuration)
            return;

        if (context.AttackTarget != null && !context.AttackTarget.IsAlive)
            context.AttackTarget = null;

        context.PendingAttackTarget = null;
        context.AttackTimer = 0f;
        context.AttackHitTime = 0f;
        context.AttackDuration = 0f;
        context.AttackDamage = 0f;
        context.AttackHitApplied = false;
        context.IsAttackAnimationLocked = false;
        context.AttackCooldown = context.Data.AttackSpeed > 0f ? 1f / context.Data.AttackSpeed : 0f;
        context.Owner.ForceRefreshAnimation();
    }

    public void CancelAttack(UnitContext context)
    {
        context.PendingAttackTarget = null;
        context.AttackTimer = 0f;
        context.AttackHitTime = 0f;
        context.AttackDuration = 0f;
        context.AttackDamage = 0f;
        context.AttackHitApplied = false;
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

    private void ApplyAttackHit(UnitContext context, UnitTargetingSystem targetingSystem)
    {
        if (context.Owner.UnitType == UnitType.Archer)
        {
            context.Owner.SpawnArrow(context.AttackStartPosition, context.AttackTargetPosition, context.AttackDamage);
            return;
        }

        var target = context.PendingAttackTarget;

        if (target == null)
            return;

        if (!target.IsAlive)
        {
            context.PendingAttackTarget = null;
            context.AttackTarget = null;
            return;
        }

        if (!targetingSystem.IsInAttackRange(context, target))
            return;

        target.TakeDamage(context.AttackDamage);

        if (!target.IsAlive)
        {
            context.PendingAttackTarget = null;
            context.AttackTarget = null;
        }
    }
}