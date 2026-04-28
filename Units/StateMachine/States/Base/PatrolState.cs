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
        context.HasReturnTarget = false;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        _combatSystem.UpdateAttack(context, _targetingSystem);

        if (context.IsAttackAnimationLocked)
            return;

        if (context.Owner.CanAttack)
        {
            if (!_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
                context.AttackTarget = _targetingSystem.FindClosestEnemy(context, context.Data.VisionRange);

            if (_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
            {
                if (!_targetingSystem.IsInsidePatrolLeash(context, context.AttackTarget.Position))
                {
                    context.AttackTarget = null;
                    ReturnToPatrolSegment(context, deltaTime);
                    return;
                }

                if (_targetingSystem.IsInAttackRange(context, context.AttackTarget))
                {
                    _movementSystem.Stop(context);
                    _combatSystem.TryAttack(context, context.AttackTarget);
                    return;
                }

                _movementSystem.MoveTo(context, context.AttackTarget.Position, deltaTime);
                return;
            }
        }

        if (context.HasReturnTarget)
        {
            var returned = _movementSystem.MoveTo(context, context.ReturnTargetPosition, deltaTime);

            if (!returned)
                return;

            context.HasReturnTarget = false;
        }

        var destination = context.PatrolToB ? context.PatrolPointB : context.PatrolPointA;
        var reached = _movementSystem.MoveTo(context, destination, deltaTime);

        if (reached)
            context.PatrolToB = !context.PatrolToB;
    }

    public void Exit(UnitContext context)
    {
    }

    private void ReturnToPatrolSegment(UnitContext context, float deltaTime)
    {
        context.ReturnTargetPosition = _targetingSystem.GetClosestPointOnPatrolSegment(context, context.Transform.position);
        context.HasReturnTarget = true;
        _movementSystem.MoveTo(context, context.ReturnTargetPosition, deltaTime);
    }
}