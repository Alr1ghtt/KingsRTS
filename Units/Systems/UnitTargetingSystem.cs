using UnityEngine;

public class UnitTargetingSystem
{
    public IAttackTarget FindClosestEnemy(UnitContext context, float searchRange)
    {
        return FindClosestEnemy(context, context.Transform.position, searchRange);
    }

    public IAttackTarget FindClosestEnemy(UnitContext context, Vector3 origin, float searchRange)
    {
        var units = UnitRegistry.Units;
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

            var distance = Vector3.Distance(origin, other.transform.position);

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

    public bool IsInsideHoldLeash(UnitContext context, Vector3 position)
    {
        var leashDistance = context.Data.AttackRange * 1.5f;
        return Vector3.Distance(context.HoldAnchorPosition, position) <= leashDistance;
    }

    public bool IsInsidePatrolLeash(UnitContext context, Vector3 position)
    {
        var leashDistance = context.Data.AttackRange * 1.5f;
        var closestPoint = GetClosestPointOnSegment(position, context.PatrolPointA, context.PatrolPointB);
        return Vector3.Distance(position, closestPoint) <= leashDistance;
    }

    public Vector3 GetClosestPointOnPatrolSegment(UnitContext context, Vector3 position)
    {
        return GetClosestPointOnSegment(position, context.PatrolPointA, context.PatrolPointB);
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

    private Vector3 GetClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var denominator = Vector3.Dot(ab, ab);

        if (denominator <= 0.0001f)
            return a;

        var t = Vector3.Dot(point - a, ab) / denominator;
        t = Mathf.Clamp01(t);

        return a + ab * t;
    }
}