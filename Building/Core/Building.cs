using UnityEngine;

public class Building : MonoBehaviour, IAttackTarget, IRepairTarget, IHealthViewTarget
{
    [SerializeField] private BuildingData _data;
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private int _ownerPlayerId;
    [SerializeField] private BuildingTeamVisual _teamVisual;
    [SerializeField] private GameObject _deathSmokePrefab;
    [SerializeField] private float _deathSmokeLifetime = 1f;

    private BuildingContext _context;
    private int _currentHealth;
    private bool _isDestroyed;

    public BuildingData Data => _data;
    public BuildingType BuildingType => _data.BuildingType;
    public TeamColor TeamColor => _teamColor;
    public int OwnerPlayerId => _ownerPlayerId;
    public int PlayerId => _ownerPlayerId;
    public int CurrentHealthInt => _currentHealth;
    public int MaxHealthInt => _data != null ? _data.MaxHealth : 0;
    public int Armor => _data != null ? _data.Armor : 0;
    public bool IsDestroyed => _isDestroyed;
    public BuildingContext Context => _context;

    public bool IsAlive => !_isDestroyed && _currentHealth > 0;
    public Vector3 Position => transform.position;
    public bool CanBeRepaired => true;
    public bool NeedsRepair => _data != null && _currentHealth < _data.MaxHealth;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _data != null ? _data.MaxHealth : 0f;

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

    private void Awake()
    {
        if (_data != null)
        {
            _currentHealth = _data.MaxHealth;
            _context = new BuildingContext(this, _data, _ownerPlayerId, _teamColor, transform);
        }

        ApplyTeamVisual();
    }

    public void TakeDamage(float damage)
    {
        if (_isDestroyed)
            return;

        if (_data == null)
            return;

        if (damage <= 0f)
            return;

        var finalDamage = Mathf.Max(Mathf.CeilToInt(damage) - Armor, 1);
        _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);

        if (_currentHealth <= 0)
            DestroyBuilding();
    }

    public void Repair(float amount)
    {
        if (_isDestroyed)
            return;

        if (_data == null)
            return;

        if (amount <= 0f)
            return;

        _currentHealth = Mathf.Min(_data.MaxHealth, _currentHealth + Mathf.CeilToInt(amount));
    }

    private void DestroyBuilding()
    {
        if (_isDestroyed)
            return;

        _isDestroyed = true;

        if (_deathSmokePrefab != null)
        {
            var smoke = Instantiate(_deathSmokePrefab, transform.position, Quaternion.identity);

            if (_deathSmokeLifetime > 0f)
                Destroy(smoke, _deathSmokeLifetime);
        }

        Destroy(gameObject);
    }

    private void ApplyTeamVisual()
    {
        if (_teamVisual != null)
            _teamVisual.Apply(_teamColor);
    }
}