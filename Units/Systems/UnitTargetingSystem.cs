using UnityEngine;

public class UnitTargetingSystem
{
    public IAttackTarget FindClosestEnemy(UnitContext context, float searchRange)
    {
        var units = UnitRegistry.Units;
        var selfPosition = context.Transform.position;
        var bestTarget = default(IAttackTarget);
        var bestDistance = float.MaxValue;

        for (int i = 0; i < units.Count; i++)
        {
            var other = units[i];
            if (other == null)
                continue;

            if (other == context.Owner)
                continue;

            if (!other.IsAlive)
                continue;

            if (other.PlayerId == context.PlayerId)
                continue;

            var distance = Vector3.Distance(selfPosition, other.transform.position);
            if (distance > searchRange)
                continue;

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = other;
        }

        return bestTarget;
    }

    public bool IsValidAttackTarget(UnitContext context, IAttackTarget target)
    {
        if (target == null)
            return false;

        if (!target.IsAlive)
            return false;

        if (target.PlayerId == context.PlayerId)
            return false;

        return true;
    }

    public bool IsInAttackRange(UnitContext context, IAttackTarget target)
    {
        if (!IsValidAttackTarget(context, target))
            return false;

        var distance = Vector3.Distance(context.Transform.position, target.Position);
        return distance <= context.Data.AttackRange;
    }

    public bool IsInRepairRange(UnitContext context, IRepairTarget target)
    {
        if (target == null)
            return false;

        if (!target.IsAlive)
            return false;

        if (!target.CanBeRepaired)
            return false;

        if (!target.NeedsRepair)
            return false;

        if (target.PlayerId != context.PlayerId)
            return false;

        var distance = Vector3.Distance(context.Transform.position, target.Position);
        return distance <= context.Data.RepairRange;
    }
}