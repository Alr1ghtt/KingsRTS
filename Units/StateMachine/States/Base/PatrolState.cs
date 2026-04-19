using UnityEngine;

public class PatrolState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;
    private readonly UnitTargetingSystem _targetingSystem;
    private readonly UnitCombatSystem _combatSystem;

    public PatrolState(UnitMovementSystem movementSystem, UnitTargetingSystem targetingSystem, UnitCombatSystem combatSystem)
    {
        _movementSystem = movementSystem;
        _targetingSystem = targetingSystem;
        _combatSystem = combatSystem;
    }

    public void Enter(UnitContext context)
    {
        context.RepairTarget = null;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        if (context.Data.CanAttack)
        {
            if (!_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
                context.AttackTarget = _targetingSystem.FindClosestEnemy(context, context.Data.VisionRange);

            if (_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
            {
                var segmentLength = Vector3.Distance(context.PatrolPointA, context.PatrolPointB);
                var maxChaseDistance = Mathf.Max(segmentLength * 2f, context.Data.VisionRange);
                var anchor = GetNearestAnchor(context);
                var targetDistanceFromAnchor = Vector3.Distance(anchor, context.AttackTarget.Position);

                if (targetDistanceFromAnchor <= maxChaseDistance)
                {
                    if (_targetingSystem.IsInAttackRange(context, context.AttackTarget))
                        _combatSystem.TryAttack(context, context.AttackTarget);
                    else
                        _movementSystem.MoveTo(context, context.AttackTarget.Position, deltaTime);

                    return;
                }

                context.AttackTarget = null;
            }
        }

        var destination = context.PatrolToB ? context.PatrolPointB : context.PatrolPointA;
        var reached = _movementSystem.MoveTo(context, destination, deltaTime);

        if (reached)
            context.PatrolToB = !context.PatrolToB;
    }

    public void Exit(UnitContext context)
    {
    }

    private Vector3 GetNearestAnchor(UnitContext context)
    {
        var distanceToA = Vector3.Distance(context.Transform.position, context.PatrolPointA);
        var distanceToB = Vector3.Distance(context.Transform.position, context.PatrolPointB);
        return distanceToA <= distanceToB ? context.PatrolPointA : context.PatrolPointB;
    }
}