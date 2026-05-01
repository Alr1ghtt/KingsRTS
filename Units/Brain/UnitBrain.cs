public class UnitBrain
{
    private UnitContext _context;
    private UnitStateMachine _stateMachine;

    private UnitMovementSystem _movementSystem;
    private UnitTargetingSystem _targetingSystem;
    private UnitCombatSystem _combatSystem;

    private UnitHealingSystem _healingSystem;
    private HealTargetState _healTargetState;

    private IdleState _idleState;
    private MoveState _moveState;
    private HoldPositionState _holdPositionState;
    private PatrolState _patrolState;
    private AttackMoveState _attackMoveState;
    private AttackTargetState _attackTargetState;
    private WorkerRepairState _workerRepairState;
    private WorkerBuildState _workerBuildState;

    public void Initialize(UnitContext context, UnitStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;

        _movementSystem = new UnitMovementSystem();
        _targetingSystem = new UnitTargetingSystem();
        _combatSystem = new UnitCombatSystem();

        _healingSystem = new UnitHealingSystem();
        _healTargetState = new HealTargetState(_movementSystem, _healingSystem);

        _idleState = new IdleState(_movementSystem, _targetingSystem, _combatSystem);
        _moveState = new MoveState(_movementSystem);
        _holdPositionState = new HoldPositionState(_movementSystem, _targetingSystem, _combatSystem);
        _patrolState = new PatrolState(_movementSystem, _targetingSystem, _combatSystem);
        _attackMoveState = new AttackMoveState(_movementSystem, _targetingSystem, _combatSystem);
        _attackTargetState = new AttackTargetState(_movementSystem, _targetingSystem, _combatSystem);
        _workerRepairState = new WorkerRepairState(_movementSystem, _targetingSystem, _combatSystem);
        _workerBuildState = new WorkerBuildState(_movementSystem);

        _stateMachine.SetState(_idleState, _context);
    }

    public void Update(float deltaTime)
    {
        _stateMachine.Update(_context, deltaTime);
    }

    public void ApplyCommand(UnitCommand command)
    {
        switch (command.Type)
        {
            case UnitCommandType.Move:
                _combatSystem.CancelAttack(_context);
                _context.MoveTargetPosition = command.PositionA;
                _context.AttackTarget = null;
                _context.RepairTarget = null;
                _stateMachine.SetState(_moveState, _context);
                break;

            case UnitCommandType.HoldPosition:
                _combatSystem.CancelAttack(_context);
                _context.HoldAnchorPosition = command.PositionA;
                _context.AttackTarget = null;
                _context.RepairTarget = null;
                _stateMachine.SetState(_holdPositionState, _context);
                break;

            case UnitCommandType.Patrol:
                _combatSystem.CancelAttack(_context);
                _context.PatrolPointA = command.PositionA;
                _context.PatrolPointB = command.PositionB;
                _context.PatrolToB = true;
                _context.AttackTarget = null;
                _context.RepairTarget = null;
                _stateMachine.SetState(_patrolState, _context);
                break;

            case UnitCommandType.AttackMove:
                _combatSystem.CancelAttack(_context);
                _context.MoveTargetPosition = command.PositionA;
                _context.AttackTarget = null;
                _context.RepairTarget = null;
                _stateMachine.SetState(_attackMoveState, _context);
                break;

            case UnitCommandType.AttackTarget:
                _combatSystem.CancelAttack(_context);
                _context.AttackTarget = command.TargetToAttack;
                _context.RepairTarget = null;
                _stateMachine.SetState(_attackTargetState, _context);
                break;

            case UnitCommandType.Repair:
                _combatSystem.CancelAttack(_context);
                _context.RepairTarget = command.TargetToRepair;
                _context.AttackTarget = null;
                _stateMachine.SetState(_workerRepairState, _context);
                break;

            case UnitCommandType.Build:
                _combatSystem.CancelAttack(_context);
                _context.MoveTargetPosition = command.PositionA;
                _context.AttackTarget = null;
                _context.RepairTarget = null;
                _stateMachine.SetState(_workerBuildState, _context);
                break;
            case UnitCommandType.Heal:
                _combatSystem.CancelAttack(_context);
                _context.HealTarget = command.TargetToHeal;
                _context.AttackTarget = null;
                _context.RepairTarget = null;
                _stateMachine.SetState(_healTargetState, _context);
                break;
        }
    }
}