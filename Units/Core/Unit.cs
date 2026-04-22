using UnityEngine;

public class Unit : MonoBehaviour, IAttackTarget, IRepairTarget
{
    [SerializeField] private UnitConfig _config;
    [SerializeField] private int _playerId;
    [SerializeField] private bool _selectableByLocalPlayer = true;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private string _idleAnimationStateName = "Idle";
    [SerializeField] private string _runAnimationStateName = "Run";
    [SerializeField] private float _animationCrossFadeDuration = 0.05f;
    [SerializeField] private int _animationLayerIndex;
    [SerializeField] private bool _drawAttackRange = true;
    [SerializeField] private bool _drawVisionRange = true;

    [SerializeField] private UnitType _unitType;
    [SerializeField] private int _ownerPlayerId;
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private float _moveSpeed = 3f;

    public UnitType UnitType => _unitType;
    public int OwnerPlayerId => _ownerPlayerId;
    public TeamColor TeamColor => _teamColor;
    public float MoveSpeed => _moveSpeed;

    public bool IsWorker()
    {
        return _unitType == UnitType.Worker;
    }

    private UnitContext _context;
    private UnitStateMachine _stateMachine;
    private UnitBrain _brain;
    private UnitSelectionMarker _selectionMarker;

    private bool _lastMovingState;
    private int _idleAnimationStateHash;
    private int _runAnimationStateHash;
    private bool _hasIdleAnimation;
    private bool _hasRunAnimation;

    public UnitContext Context => _context;
    public UnitData Data => _config.Data;
    public int PlayerId => _playerId;
    public bool SelectableByLocalPlayer => _selectableByLocalPlayer;
    public bool IsAlive => _context != null && _context.CurrentHealth > 0f;
    public Vector3 Position => transform.position;
    public bool CanBeRepaired => Data.CanBeRepaired;
    public bool NeedsRepair => _context != null && _context.CurrentHealth < Data.MaxHealth;

    private void Awake()
    {
        Initialize();
        UnitRegistry.Register(this);
    }

    private void OnDestroy()
    {
        UnitRegistry.Unregister(this);
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    private void Initialize()
    {
        _context = new UnitContext();
        _context.Initialize(this, _config.Data, transform, _playerId);

        _stateMachine = new UnitStateMachine();
        _brain = new UnitBrain();
        _brain.Initialize(_context, _stateMachine);

        _selectionMarker = GetComponent<UnitSelectionMarker>();

        _idleAnimationStateHash = Animator.StringToHash(_idleAnimationStateName);
        _runAnimationStateHash = Animator.StringToHash(_runAnimationStateName);

        _hasIdleAnimation = HasAnimationState(_idleAnimationStateHash);
        _hasRunAnimation = HasAnimationState(_runAnimationStateHash);

        _lastMovingState = false;
        PlayIdleAnimationImmediate();
    }

    private void Tick(float deltaTime)
    {
        if (_context.AttackCooldown > 0f)
            _context.AttackCooldown -= deltaTime;

        _brain.Update(deltaTime);
        UpdateVisualDirection();
        UpdateAnimation();
    }

    private void UpdateVisualDirection()
    {
        if (_visualRoot == null)
            return;

        if (!_context.IsMoving)
            return;

        var directionX = _context.MoveDirection.x;
        if (Mathf.Abs(directionX) <= 0.001f)
            return;

        var scale = _visualRoot.localScale;
        var absX = Mathf.Abs(scale.x);
        scale.x = directionX < 0f ? -absX : absX;
        _visualRoot.localScale = scale;
    }

    private void UpdateAnimation()
    {
        if (_animator == null)
            return;

        if (_context.IsMoving == _lastMovingState)
            return;

        _lastMovingState = _context.IsMoving;

        if (_context.IsMoving)
            PlayRunAnimation();
        else
            PlayIdleAnimation();
    }

    private void PlayIdleAnimationImmediate()
    {
        if (_animator == null)
            return;

        if (!_hasIdleAnimation)
            return;

        _animator.Play(_idleAnimationStateHash, _animationLayerIndex, 0f);
    }

    private void PlayIdleAnimation()
    {
        if (_animator == null)
            return;

        if (!_hasIdleAnimation)
            return;

        _animator.CrossFade(_idleAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
    }

    private void PlayRunAnimation()
    {
        if (_animator == null)
            return;

        if (!_hasRunAnimation)
            return;

        _animator.CrossFade(_runAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
    }

    private bool HasAnimationState(int stateHash)
    {
        if (_animator == null)
            return false;

        if (_animationLayerIndex < 0 || _animationLayerIndex >= _animator.layerCount)
            return false;

        return _animator.HasState(_animationLayerIndex, stateHash);
    }

    public void ApplyCommand(UnitCommand command)
    {
        _brain.ApplyCommand(command);
    }

    public void SetSelected(bool selected)
    {
        _context.IsSelected = selected;

        if (_selectionMarker != null)
            _selectionMarker.SetSelected(selected);
    }

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
        _context.PlayerId = playerId;
    }

    public void TakeDamage(float damage)
    {
        var finalDamage = Mathf.Max(damage - Data.Armor, 0f);
        _context.CurrentHealth -= finalDamage;

        if (_context.CurrentHealth <= 0f)
            Destroy(gameObject);
    }

    public void Repair(float amount)
    {
        _context.CurrentHealth = Mathf.Min(_context.CurrentHealth + amount, Data.MaxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        if (_config == null || _config.Data == null)
            return;

        var position = transform.position;

        if (_drawAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, _config.Data.AttackRange);
        }

        if (_drawVisionRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, _config.Data.VisionRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, _config.Data.Radius);
    }
}