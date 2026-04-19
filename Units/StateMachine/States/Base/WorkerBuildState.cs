public class WorkerBuildState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;

    public WorkerBuildState(UnitMovementSystem movementSystem)
    {
        _movementSystem = movementSystem;
    }

    public void Enter(UnitContext context)
    {
        context.AttackTarget = null;
        context.RepairTarget = null;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        if (!context.Data.CanBuild)
        {
            _movementSystem.Stop(context);
            return;
        }

        var reached = _movementSystem.MoveTo(context, context.MoveTargetPosition, deltaTime);
        if (reached)
            _movementSystem.Stop(context);
    }

    public void Exit(UnitContext context)
    {
    }
}