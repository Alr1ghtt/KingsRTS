public class AttackTargetState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;
    private readonly UnitTargetingSystem _targetingSystem;
    private readonly UnitCombatSystem _combatSystem;

    public AttackTargetState(UnitMovementSystem movementSystem, UnitTargetingSystem targetingSystem, UnitCombatSystem combatSystem)
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
        if (!_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
        {
            _movementSystem.Stop(context);
            return;
        }

        if (_targetingSystem.IsInAttackRange(context, context.AttackTarget))
            _combatSystem.TryAttack(context, context.AttackTarget);
        else
            _movementSystem.MoveTo(context, context.AttackTarget.Position, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }
}