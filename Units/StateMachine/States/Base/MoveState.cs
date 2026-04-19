public class MoveState : IUnitState
{
    private readonly UnitMovementSystem _movementSystem;

    public MoveState(UnitMovementSystem movementSystem)
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
        _movementSystem.MoveTo(context, context.MoveTargetPosition, deltaTime);
    }

    public void Exit(UnitContext context)
    {
    }
}