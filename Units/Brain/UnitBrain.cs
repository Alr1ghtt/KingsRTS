using UnityEngine;

public class UnitBrain
{
    private UnitContext _context;
    private UnitStateMachine _stateMachine;

    private UnitCommand _currentCommand;

    private IUnitState _idleState = new IdleState();
    private IUnitState _moveState = new MoveState();
    private IUnitState _attackState = new AttackState();
    private IUnitState _holdState = new HoldPositionState();

    public void Initialize(UnitContext context, UnitStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;

        _stateMachine.SetState(_idleState, _context);
    }

    public void Update(float deltaTime)
    {
        ProcessCommand();
        _stateMachine.Update(_context, deltaTime);
    }

    public void SetCommand(UnitCommand command)
    {
        _currentCommand = command;
    }

    private void ProcessCommand()
    {
        if (_currentCommand == null)
            return;

        switch (_currentCommand.Type)
        {
            case UnitCommandType.Move:
                _context.TargetPosition = _currentCommand.Position;
                _context.TargetUnit = null;
                _stateMachine.SetState(_moveState, _context);
                break;

            case UnitCommandType.Attack:
                _context.TargetUnit = _currentCommand.Target;
                _stateMachine.SetState(_attackState, _context);
                break;

            case UnitCommandType.HoldPosition:
                _stateMachine.SetState(_holdState, _context);
                break;
        }

        _currentCommand = null;
    }
}