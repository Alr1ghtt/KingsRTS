public class HealTargetState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;
    private readonly UnitHealingSystem _healingSystem;

    public HealTargetState(UnitMovementSystem movementSystem, UnitHealingSystem healingSystem)
    {
        _movementSystem = movementSystem;
        _healingSystem = healingSystem;
    }

    public void Enter(UnitContext context)
    {
        context.AttackTarget = null;
        context.RepairTarget = null;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        if (!_healingSystem.IsValidHealTarget(context, context.HealTarget))
        {
            _movementSystem.Stop(context);
            return;
        }

        if (_healingSystem.IsInHealRange(context, context.HealTarget))
        {
            _movementSystem.Stop(context);
            _healingSystem.TryHeal(context, context.HealTarget);
            return;
        }

        _movementSystem.MoveTo(context, context.HealTarget.Position, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }
}