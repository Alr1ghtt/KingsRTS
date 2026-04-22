using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData _data;
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private int _ownerPlayerId;
    [SerializeField] private BuildingTeamVisual _teamVisual;

    private BuildingContext _context;
    private int _currentHealth;
    private bool _isDestroyed;

    public BuildingData Data => _data;
    public BuildingType BuildingType => _data.BuildingType;
    public TeamColor TeamColor => _teamColor;
    public int OwnerPlayerId => _ownerPlayerId;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _data.MaxHealth;
    public int Armor => _data.Armor;
    public bool IsDestroyed => _isDestroyed;
    public BuildingContext Context => _context;

    public void Initialize(BuildingData data, int ownerPlayerId, TeamColor teamColor)
    {
        _data = data;
        _ownerPlayerId = ownerPlayerId;
        _teamColor = teamColor;
        _currentHealth = _data.MaxHealth;
        _isDestroyed = false;
        _context = new BuildingContext(this, _data, _ownerPlayerId, _teamColor, transform);

        ApplyTeamVisual();
    }

    public bool CanProduce(BuildingUnitType unitType)
    {
        if (_data == null)
            return false;

        return _data.CanProduce(unitType);
    }

    public bool CanGarrison(BuildingUnitType unitType)
    {
        if (_data == null)
            return false;

        return _data.CanGarrison(unitType);
    }

    public bool CanUpgradeUnits()
    {
        if (_data == null)
            return false;

        return _data.CanUpgradeUnits;
    }

    public void RestoreFullHealth()
    {
        if (_data == null)
            return;

        _currentHealth = _data.MaxHealth;
        _isDestroyed = false;
    }

    public void TakeDamage(int damage)
    {
        if (_isDestroyed || _data == null)
            return;

        int finalDamage = Mathf.Max(1, damage - _data.Armor);
        _currentHealth -= finalDamage;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            _isDestroyed = true;
        }
    }

    public void Heal(int amount)
    {
        if (_isDestroyed || _data == null)
            return;

        _currentHealth = Mathf.Min(_currentHealth + amount, _data.MaxHealth);
    }

    private void Awake()
    {
        if (_data != null)
        {
            _currentHealth = _data.MaxHealth;
            _context = new BuildingContext(this, _data, _ownerPlayerId, _teamColor, transform);
        }

        ApplyTeamVisual();
    }

    private void ApplyTeamVisual()
    {
        if (_teamVisual == null)
            return;

        _teamVisual.Apply(_teamColor);
    }
}