using UnityEngine;

public class UnitContext
{
    private UnitData _data;
    private Transform _transform;

    private float _currentHealth;

    private Unit _targetUnit;
    private Vector3 _targetPosition;

    private float _attackCooldown;

    private bool _isHoldingPosition;

    public UnitData Data => _data;
    public Transform Transform => _transform;

    public float CurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }

    public Unit TargetUnit
    {
        get => _targetUnit;
        set => _targetUnit = value;
    }

    public Vector3 TargetPosition
    {
        get => _targetPosition;
        set => _targetPosition = value;
    }

    public float AttackCooldown
    {
        get => _attackCooldown;
        set => _attackCooldown = value;
    }

    public bool IsHoldingPosition
    {
        get => _isHoldingPosition;
        set => _isHoldingPosition = value;
    }

    public void Initialize(UnitData data, Transform transform)
    {
        _data = data;
        _transform = transform;
        _currentHealth = data.MaxHealth;
    }
}