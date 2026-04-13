using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitData _data;

    private UnitContext _context;

    private UnitStateMachine _stateMachine;
    private UnitBrain _brain;
    public UnitContext Context => _context;
    public UnitData Data => _data;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
        _brain.Update(Time.deltaTime);
    }

    private void Initialize()
    {
        _stateMachine = new UnitStateMachine();
        _brain = new UnitBrain();

        _brain.Initialize(_context, _stateMachine);

        _context = new UnitContext();
        _context.Initialize(_data, transform);
    }

    private void Tick(float deltaTime)
    {
        if (_context.AttackCooldown > 0f)
            _context.AttackCooldown -= deltaTime;
    }

    public void SetTargetPosition(Vector3 position)
    {
        _context.TargetPosition = position;
        _context.TargetUnit = null;
        _context.IsHoldingPosition = false;
    }

    public void SetTargetUnit(Unit target)
    {
        _context.TargetUnit = target;
        _context.IsHoldingPosition = false;
    }

    public void HoldPosition()
    {
        _context.TargetUnit = null;
        _context.IsHoldingPosition = true;
    }

    public void TakeDamage(float damage)
    {
        var finalDamage = Mathf.Max(damage - _data.Armor, 0f);
        _context.CurrentHealth -= finalDamage;

        if (_context.CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}