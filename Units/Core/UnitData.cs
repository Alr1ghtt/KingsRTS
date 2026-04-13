using UnityEngine;

public enum UnitType
{
    Archer,
    Spearman,
    Monk,
    Warrior,
    Worker
}

public enum TeamColor
{
    Black,
    Blue,
    Purple,
    Red,
    Yellow
}

[System.Serializable]
public class UnitData
{
    [SerializeField] private UnitType _type;
    [SerializeField] private TeamColor _teamColor;

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _armor;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _visionRange;

    public UnitType Type => _type;
    public TeamColor TeamColor => _teamColor;

    public float MaxHealth => _maxHealth;
    public float Armor => _armor;
    public float AttackSpeed => _attackSpeed;
    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float MoveSpeed => _moveSpeed;
    public float VisionRange => _visionRange;
}