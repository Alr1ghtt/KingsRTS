public class UnitStateMachine
{
    private IUnitState _currentState;

    public IUnitState CurrentState => _currentState;

    public void SetState(IUnitState newState, UnitContext context)
    {
        _currentState?.Exit(context);
        _currentState = newState;
        _currentState?.Enter(context);
    }

    public void Update(UnitContext context, float deltaTime)
    {
        _currentState?.Update(context, deltaTime);
    }
} 