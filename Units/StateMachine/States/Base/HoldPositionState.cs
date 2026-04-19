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
    }

    public void Update(UnitContext context, float deltaTime)
    {
        if (!context.Data.CanAttack)
        {
            _movementSystem.Stop(context);
            return;
        }

        if (!_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
            context.AttackTarget = _targetingSystem.FindClosestEnemy(context, context.Data.AttackRange);

        if (_targetingSystem.IsInAttackRange(context, context.AttackTarget))
            _combatSystem.TryAttack(context, context.AttackTarget);

        _movementSystem.Stop(context);
    }

    public void Exit(UnitContext context)
    {
    }
}