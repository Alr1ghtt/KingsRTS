public class HoldPositionState : IUnitState
{
    public void Enter(UnitContext context)
    {
        context.IsHoldingPosition = true;
    }

    public void Update(UnitContext context, float deltaTime) { }

    public void Exit(UnitContext context)
    {
        context.IsHoldingPosition = false;
    }
}