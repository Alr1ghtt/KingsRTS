using UnityEngine;

public class UnitContext
{
    private UnitData _data;
    private Transform _transform;
    private Unit _owner;

    private float _currentHealth;
    private float _attackCooldown;
    private float _attackTimer;
    private float _attackHitTime;
    private float _attackDuration;
    private float _attackDamage;
    private bool _attackHitApplied;

    private Vector3 _moveTargetPosition;
    private Vector3 _holdAnchorPosition;
    private Vector3 _patrolPointA;
    private Vector3 _patrolPointB;
    private Vector3 _returnTargetPosition;
    private Vector3 _attackStartPosition;
    private Vector3 _attackTargetPosition;
    private bool _patrolToB = true;
    private bool _hasReturnTarget;
    private bool _isAttackAnimationLocked;

    private IAttackTarget _attackTarget;
    private IAttackTarget _pendingAttackTarget;
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

    public float AttackTimer
    {
        get => _attackTimer;
        set => _attackTimer = value;
    }

    public float AttackHitTime
    {
        get => _attackHitTime;
        set => _attackHitTime = value;
    }

    public float AttackDuration
    {
        get => _attackDuration;
        set => _attackDuration = value;
    }

    public float AttackDamage
    {
        get => _attackDamage;
        set => _attackDamage = value;
    }

    public bool AttackHitApplied
    {
        get => _attackHitApplied;
        set => _attackHitApplied = value;
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

    public Vector3 ReturnTargetPosition
    {
        get => _returnTargetPosition;
        set => _returnTargetPosition = value;
    }

    public Vector3 AttackStartPosition
    {
        get => _attackStartPosition;
        set => _attackStartPosition = value;
    }

    public Vector3 AttackTargetPosition
    {
        get => _attackTargetPosition;
        set => _attackTargetPosition = value;
    }

    public bool PatrolToB
    {
        get => _patrolToB;
        set => _patrolToB = value;
    }

    public bool HasReturnTarget
    {
        get => _hasReturnTarget;
        set => _hasReturnTarget = value;
    }

    public bool IsAttackAnimationLocked
    {
        get => _isAttackAnimationLocked;
        set => _isAttackAnimationLocked = value;
    }

    public IAttackTarget AttackTarget
    {
        get => _attackTarget;
        set => _attackTarget = value;
    }

    public IAttackTarget PendingAttackTarget
    {
        get => _pendingAttackTarget;
        set => _pendingAttackTarget = value;
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
        _attackCooldown = 0f;
        _attackTimer = 0f;
        _attackHitTime = 0f;
        _attackDuration = 0f;
        _attackDamage = 0f;
        _attackHitApplied = false;
        _moveTargetPosition = transform.position;
        _holdAnchorPosition = transform.position;
        _patrolPointA = transform.position;
        _patrolPointB = transform.position;
        _returnTargetPosition = transform.position;
        _attackStartPosition = transform.position;
        _attackTargetPosition = transform.position;
        _moveDirection = Vector3.zero;
        _isMoving = false;
        _currentVelocity = Vector3.zero;
        _hasReturnTarget = false;
        _isAttackAnimationLocked = false;
    }
}