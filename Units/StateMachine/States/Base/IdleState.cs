public class IdleState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;

    public IdleState(UnitMovementSystem movementSystem)
    {
        _movementSystem = movementSystem;
    }

    public void Enter(UnitContext context)
    {
        _movementSystem.Stop(context);
        context.AttackTarget = null;
        context.RepairTarget = null;
    }

    public void Update(UnitContext context, float deltaTime)
    {
        _movementSystem.Stop(context);
    }

    public void Exit(UnitContext context)
    {
    }
}