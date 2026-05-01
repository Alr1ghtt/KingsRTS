using System;
using UnityEngine;

[Serializable]
public class UnitData
{
    [SerializeField] private UnitType _type;
    [SerializeField] private TeamColor _teamColor;

    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _armor = 0f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private float _attackRange = 1.25f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _visionRange = 8f;
    [SerializeField] private float _radius = 0.35f;
    [SerializeField] private float _separationWeight = 2.4f;
    [SerializeField] private float _separationRangeMultiplier = 2.2f;

    [SerializeField] private bool _canAttack = true;
    [SerializeField] private bool _canRepair;
    [SerializeField] private bool _canBuild;
    [SerializeField] private bool _canBeRepaired;
    [SerializeField] private float _repairRange = 1.1f;
    [SerializeField] private float _repairRate = 18f;

    [SerializeField] private float _maxMana = 100f;
    [SerializeField] private float _manaRegenRate = 3f;
    [SerializeField] private float _healManaCost = 10f;

    [SerializeField] private UnitCost _cost;

    public UnitType Type => _type;
    public TeamColor TeamColor => _teamColor;
    public float MaxHealth => _maxHealth;
    public float Armor => _armor;
    public float AttackSpeed => _attackSpeed;
    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float MoveSpeed => _moveSpeed;
    public float VisionRange => _visionRange;
    public float Radius => _radius;
    public float SeparationWeight => _separationWeight;
    public float SeparationRangeMultiplier => _separationRangeMultiplier;
    public bool CanAttack => _canAttack;
    public bool CanRepair => _canRepair;
    public bool CanBuild => _canBuild;
    public bool CanBeRepaired => _canBeRepaired;
    public float RepairRange => _repairRange;
    public float RepairRate => _repairRate;

    public float MaxMana => _maxMana;
    public float ManaRegenRate => _manaRegenRate;
    public float HealManaCost => _healManaCost;
    public UnitCost Cost => _cost;
}