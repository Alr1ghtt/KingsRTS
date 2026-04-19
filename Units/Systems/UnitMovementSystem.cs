using UnityEngine;

public class UnitMovementSystem
{
    private const float StoppingDistanceMultiplier = 0.2f;
    private const float ArrivalSlowdownMultiplier = 3.5f;
    private const float VelocitySmoothTime = 0.08f;

    public bool MoveTo(UnitContext context, Vector3 targetPosition, float deltaTime)
    {
        var currentPosition = context.Transform.position;
        var toTarget = targetPosition - currentPosition;
        toTarget.z = 0f;

        var stoppingDistance = context.Data.Radius + StoppingDistanceMultiplier;
        var distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= stoppingDistance)
        {
            context.IsMoving = false;
            context.MoveDirection = Vector3.zero;
            context.CurrentVelocity = Vector3.zero;
            return true;
        }

        var desiredDirection = distanceToTarget > 0.0001f ? toTarget / distanceToTarget : Vector3.zero;
        var separation = CalculateSeparation(context);
        var steering = desiredDirection + separation * context.Data.SeparationWeight;

        if (steering.sqrMagnitude <= 0.0001f)
            steering = desiredDirection;

        steering.Normalize();

        var desiredSpeed = context.Data.MoveSpeed;
        var slowdownDistance = Mathf.Max(stoppingDistance * ArrivalSlowdownMultiplier, 0.5f);

        if (distanceToTarget < slowdownDistance)
            desiredSpeed *= distanceToTarget / slowdownDistance;

        var desiredVelocity = steering * desiredSpeed;
        var smoothedVelocity = Vector3.Lerp(context.CurrentVelocity, desiredVelocity, Mathf.Clamp01(deltaTime / VelocitySmoothTime));
        smoothedVelocity.z = 0f;

        var nextPosition = currentPosition + smoothedVelocity * deltaTime;
        nextPosition.z = currentPosition.z;

        context.Transform.position = nextPosition;
        context.CurrentVelocity = smoothedVelocity;
        context.IsMoving = smoothedVelocity.sqrMagnitude > 0.0001f;
        context.MoveDirection = context.IsMoving ? smoothedVelocity.normalized : Vector3.zero;

        ResolveOverlaps(context);

        return false;
    }

    public void Stop(UnitContext context)
    {
        context.IsMoving = false;
        context.MoveDirection = Vector3.zero;
        context.CurrentVelocity = Vector3.zero;
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