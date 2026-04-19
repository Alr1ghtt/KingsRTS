public class AttackMoveState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;
    private readonly UnitTargetingSystem _targetingSystem;
    private readonly UnitCombatSystem _combatSystem;

    public AttackMoveState(UnitMovementSystem movementSystem, UnitTargetingSystem targetingSystem, UnitCombatSystem combatSystem)
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
                if (_targetingSystem.IsInAttackRange(context, context.AttackTarget))
                    _combatSystem.TryAttack(context, context.AttackTarget);
                else
                    _movementSystem.MoveTo(context, context.AttackTarget.Position, deltaTime);

                return;
            }
        }

        _movementSystem.MoveTo(context, context.MoveTargetPosition, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }
}