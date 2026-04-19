public class UnitStateMachine
{
    private IUnitState _currentState;

    public IUnitState CurrentState => _currentState;

    public void SetState(IUnitState state, UnitContext context)
    {
        if (_currentState == state)
            return;

        _currentState?.Exit(context);
        _currentState = state;
        _currentState?.Enter(context);
    }

    public void Update(UnitContext context, float deltaTime)
    {
        _currentState?.Update(context, deltaTime);
    }
}