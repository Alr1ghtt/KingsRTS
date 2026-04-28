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
        context.HasReturnTarget = false;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        _combatSystem.UpdateAttack(context, _targetingSystem, deltaTime);

        if (context.IsAttackAnimationLocked)
            return;

        if (!context.Owner.CanAttack)
        {
            _movementSystem.Stop(context);
            return;
        }

        if (!_targetingSystem.IsValidAttackTarget(context, context.AttackTarget))
        {
            _movementSystem.Stop(context);
            return;
        }

        if (_targetingSystem.IsInAttackRange(context, context.AttackTarget))
        {
            _movementSystem.Stop(context);
            _combatSystem.TryAttack(context, context.AttackTarget);
            return;
        }

        _movementSystem.MoveTo(context, context.AttackTarget.Position, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }
}