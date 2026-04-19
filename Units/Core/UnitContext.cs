using UnityEngine;

public class UnitContext
{
    private UnitData _data;
    private Transform _transform;
    private Unit _owner;

    private float _currentHealth;
    private float _attackCooldown;

    private Vector3 _moveTargetPosition;
    private Vector3 _holdAnchorPosition;
    private Vector3 _patrolPointA;
    private Vector3 _patrolPointB;
    private bool _patrolToB = true;

    private IAttackTarget _attackTarget;
    private IRepairTarget _repairTarget;

    private int _playerId;
    private bool _isSelected;

    private Vector3 _moveDirection;
    private bool _isMoving;
    private Vector3 _currentVelocity;

    public Unit Owner => _owner;
    public UnitData Data => _data;
    public Transform Transform => _transform;

    public float CurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }

    public float AttackCooldown
    {
        get => _attackCooldown;
        set => _attackCooldown = value;
    }

    public Vector3 MoveTargetPosition
    {
        get => _moveTargetPosition;
        set => _moveTargetPosition = value;
    }

    public Vector3 HoldAnchorPosition
    {
        get => _holdAnchorPosition;
        set => _holdAnchorPosition = value;
    }

    public Vector3 PatrolPointA
    {
        get => _patrolPointA;
        set => _patrolPointA = value;
    }

    public Vector3 PatrolPointB
    {
        get => _patrolPointB;
        set => _patrolPointB = value;
    }

    public bool PatrolToB
    {
        get => _patrolToB;
        set => _patrolToB = value;
    }

    public IAttackTarget AttackTarget
    {
        get => _attackTarget;
        set => _attackTarget = value;
    }

    public IRepairTarget RepairTarget
    {
        get => _repairTarget;
        set => _repairTarget = value;
    }

    public int PlayerId
    {
        get => _playerId;
        set => _playerId = value;
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }

    public Vector3 MoveDirection
    {
        get => _moveDirection;
        set => _moveDirection = value;
    }

    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

    public Vector3 CurrentVelocity
    {
        get => _currentVelocity;
        set => _currentVelocity = value;
    }

    public void Initialize(Unit owner, UnitData data, Transform transform, int playerId)
    {
        _owner = owner;
        _data = data;
        _transform = transform;
        _playerId = playerId;
        _currentHealth = data.MaxHealth;
        _moveTargetPosition = transform.position;
        _holdAnchorPosition = transform.position;
        _patrolPointA = transform.position;
        _patrolPointB = transform.position;
        _moveDirection = Vector3.zero;
        _isMoving = false;
        _currentVelocity = Vector3.zero;
    }
}