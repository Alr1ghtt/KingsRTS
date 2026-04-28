public class HoldPositionState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;
    private readonly UnitTargetingSystem _targetingSystem;
    private readonly UnitCombatSystem _combatSystem;

    public HoldPositionState(UnitMovementSystem movementSystem, UnitTargetingSystem targetingSystem, UnitCombatSystem combatSystem)
    {
        _movementSystem = movementSystem;
        _targetingSystem = targetingSystem;
        _combatSystem = combatSystem;
    }

    public void Enter(UnitContext context)
    {
        _movementSystem.Stop(context);
        context.RepairTarget = null;
        context.HasReturnTarget = false;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        _combatSystem.UpdateAttack(context, _targetingSystem, deltaTime);

        if (context.IsAttackAnimationLocked)
            return;

        if (!context.Owner.CanAttack)
        {
            ReturnToHoldAnchor(context, deltaTime);
            return;
        }

        if (!_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
            context.AttackTarget = _targetingSystem.FindClosestEnemy(context, context.HoldAnchorPosition, context.Data.VisionRange);

        if (_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
        {
            if (!_targetingSystem.IsInsideHoldLeash(context, context.AttackTarget.Position))
            {
                context.AttackTarget = null;
                ReturnToHoldAnchor(context, deltaTime);
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

        ReturnToHoldAnchor(context, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }

    private void ReturnToHoldAnchor(UnitContext context, float deltaTime)
    {
        _movementSystem.MoveTo(context, context.HoldAnchorPosition, deltaTime);
    }
}