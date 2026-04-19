public class WorkerRepairState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;
    private readonly UnitTargetingSystem _targetingSystem;
    private readonly UnitCombatSystem _combatSystem;

    public WorkerRepairState(UnitMovementSystem movementSystem, UnitTargetingSystem targetingSystem, UnitCombatSystem combatSystem)
    {
        _movementSystem = movementSystem;
        _targetingSystem = targetingSystem;
        _combatSystem = combatSystem;
    }

    public void Enter(UnitContext context)
    {
        context.AttackTarget = null;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        if (!context.Data.CanRepair)
        {
            _movementSystem.Stop(context);
            return;
        }

        if (context.RepairTarget == null || !context.RepairTarget.IsAlive || !context.RepairTarget.CanBeRepaired || !context.RepairTarget.NeedsRepair || context.RepairTarget.PlayerId != context.PlayerId)
        {
            _movementSystem.Stop(context);
            return;
        }

        if (_targetingSystem.IsInRepairRange(context, context.RepairTarget))
        {
            _movementSystem.Stop(context);
            _combatSystem.Repair(context, context.RepairTarget, deltaTime);
            return;
        }

        _movementSystem.MoveTo(context, context.RepairTarget.Position, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }
}