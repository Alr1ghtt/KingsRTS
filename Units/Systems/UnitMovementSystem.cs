using UnityEngine;

public class UnitMovementSystem
{
    private const float StoppingDistanceMultiplier = 0.2f;

    public bool MoveTo(UnitContext context, Vector3 targetPosition, float deltaTime)
    {
        var currentPosition = context.Transform.position;
        var toTarget = targetPosition - currentPosition;
        toTarget.z = 0f;

        var stoppingDistance = context.Data.Radius + StoppingDistanceMultiplier;
        var distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= stoppingDistance)
        {
            context.Transform.position = new Vector3(targetPosition.x, targetPosition.y, currentPosition.z);
            context.IsMoving = false;
            context.MoveDirection = Vector3.zero;
            context.CurrentVelocity = Vector3.zero;
            return true;
        }

        var desiredDirection = distanceToTarget > 0.0001f ? toTarget / distanceToTarget : Vector3.zero;

        var shouldUseLocalAvoidance = ShouldUseLocalAvoidance(context);
        var steering = desiredDirection;

        if (shouldUseLocalAvoidance)
        {
            var separation = CalculateSeparation(context);
            steering = desiredDirection + separation * context.Data.SeparationWeight;

            if (steering.sqrMagnitude <= 0.0001f)
                steering = desiredDirection;

            steering.Normalize();
        }

        var moveSpeed = context.Data.MoveSpeed;
        var step = moveSpeed * deltaTime;

        if (step >= distanceToTarget - stoppingDistance)
        {
            var clampedPosition = currentPosition + steering * Mathf.Max(distanceToTarget - stoppingDistance, 0f);
            clampedPosition.z = currentPosition.z;

            context.Transform.position = clampedPosition;
            context.CurrentVelocity = Vector3.zero;
            context.IsMoving = false;
            context.MoveDirection = Vector3.zero;

            if (shouldUseLocalAvoidance)
                ResolveOverlaps(context);

            return true;
        }

        var velocity = steering * moveSpeed;
        velocity.z = 0f;

        var nextPosition = currentPosition + velocity * deltaTime;
        nextPosition.z = currentPosition.z;

        context.Transform.position = nextPosition;
        context.CurrentVelocity = velocity;
        context.IsMoving = true;
        context.MoveDirection = velocity.normalized;

        if (shouldUseLocalAvoidance)
            ResolveOverlaps(context);

        return false;
    }

    public void Stop(UnitContext context)
    {
        context.IsMoving = false;
        context.MoveDirection = Vector3.zero;
        context.CurrentVelocity = Vector3.zero;
    }

    private bool ShouldUseLocalAvoidance(UnitContext context)
    {
        if (context == null || context.Owner == null)
            return true;

        var workerConstructionAgent = context.Owner.GetComponent<WorkerConstructionAgent>();
        if (workerConstructionAgent == null)
            return true;

        return !workerConstructionAgent.DisableLocalAvoidance;
    }

    private Vector3 CalculateSeparation(UnitContext context)
    {
        var units = UnitRegistry.Units;
        var selfPosition = context.Transform.position;
        var selfRadius = context.Data.Radius;
        var separationRange = selfRadius * context.Data.SeparationRangeMultiplier + selfRadius;

        var result = Vector3.zero;
        var neighborCount = 0;

        for (int i = 0; i < units.Count; i++)
        {
            var other = units[i];
            if (other == null)
                continue;

            if (other == context.Owner)
                continue;

            if (other.PlayerId != context.PlayerId)
                continue;

            var otherPosition = other.transform.position;
            var offset = selfPosition - otherPosition;
            offset.z = 0f;

            var distance = offset.magnitude;
            if (distance <= 0.0001f)
                continue;

            var combinedRadius = selfRadius + other.Data.Radius;
            var effectiveRange = Mathf.Max(separationRange, combinedRadius * 1.5f);

            if (distance > effectiveRange)
                continue;

            var strength = 1f - distance / effectiveRange;
            result += offset.normalized * strength;
            neighborCount++;
        }

        if (neighborCount == 0)
            return Vector3.zero;

        result /= neighborCount;
        result.z = 0f;
        return result;
    }

    private void ResolveOverlaps(UnitContext context)
    {
        var units = UnitRegistry.Units;
        var selfPosition = context.Transform.position;
        var selfRadius = context.Data.Radius;

        for (int i = 0; i < units.Count; i++)
        {
            var other = units[i];
            if (other == null)
                continue;

            if (other == context.Owner)
                continue;

            if (other.PlayerId != context.PlayerId)
                continue;

            var otherPosition = other.transform.position;
            var offset = selfPosition - otherPosition;
            offset.z = 0f;

            var distance = offset.magnitude;
            var minDistance = selfRadius + other.Data.Radius;

            if (distance >= minDistance)
                continue;

            if (distance <= 0.0001f)
                offset = Random.insideUnitCircle.normalized;

            var pushDirection = offset.normalized;
            var penetration = minDistance - Mathf.Max(distance, 0.0001f);
            var push = pushDirection * (penetration * 0.5f);

            selfPosition += push;
            selfPosition.z = context.Transform.position.z;
        }

        context.Transform.position = selfPosition;
    }
}