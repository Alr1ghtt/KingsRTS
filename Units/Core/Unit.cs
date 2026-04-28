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
    [SerializeField] private string _buildAnimationStateName = "Build";
    [SerializeField] private string _attackAnimationStateName = "Attack";
    [SerializeField] private string _warriorAttackAnimationStateNameA = "Attack1";
    [SerializeField] private string _warriorAttackAnimationStateNameB = "Attack2";
    [SerializeField] private string _spearmanAttackRightAnimationStateName = "AttackRight";
    [SerializeField] private string _spearmanAttackUpAnimationStateName = "AttackUp";
    [SerializeField] private string _spearmanAttackDownAnimationStateName = "AttackDown";
    [SerializeField] private string _spearmanAttackUpRightAnimationStateName = "AttackUpRight";
    [SerializeField] private string _spearmanAttackDownRightAnimationStateName = "AttackDownRight";
    [SerializeField] private float _warriorAttackDuration = 0.4f;
    [SerializeField] private float _spearmanAttackDuration = 0.3f;
    [SerializeField] private float _archerAttackDuration = 0.8f;
    [SerializeField] private float _defaultAttackDuration = 0.4f;
    [SerializeField] private float _animationCrossFadeDuration = 0.05f;
    [SerializeField] private int _animationLayerIndex;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private Transform _arrowSpawnPoint;
    [SerializeField] private Vector2 _arrowSpawnPointRightLocalPosition = new Vector2(0.5f, 0.5f);
    [SerializeField] private GameObject _deathSmokePrefab;
    [SerializeField] private float _deathSmokeLifetime = 2f;
    [SerializeField] private bool _drawAttackRange = true;
    [SerializeField] private bool _drawVisionRange = true;

    [SerializeField] private UnitType _unitType;
    [SerializeField] private int _ownerPlayerId;
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private float _moveSpeed = 3f;

    private UnitContext _context;
    private UnitStateMachine _stateMachine;
    private UnitBrain _brain;
    private UnitSelectionMarker _selectionMarker;
    private WorkerConstructionAgent _workerConstructionAgent;

    private bool _lastMovingState;
    private int _idleAnimationStateHash;
    private int _runAnimationStateHash;
    private int _buildAnimationStateHash;
    private int _attackAnimationStateHash;
    private int _warriorAttackAnimationStateHashA;
    private int _warriorAttackAnimationStateHashB;
    private int _spearmanAttackRightAnimationStateHash;
    private int _spearmanAttackUpAnimationStateHash;
    private int _spearmanAttackDownAnimationStateHash;
    private int _spearmanAttackUpRightAnimationStateHash;
    private int _spearmanAttackDownRightAnimationStateHash;
    private bool _hasIdleAnimation;
    private bool _hasRunAnimation;
    private bool _hasBuildAnimation;
    private bool _hasAttackAnimation;
    private bool _hasWarriorAttackAnimationA;
    private bool _hasWarriorAttackAnimationB;
    private bool _hasSpearmanAttackRightAnimation;
    private bool _hasSpearmanAttackUpAnimation;
    private bool _hasSpearmanAttackDownAnimation;
    private bool _hasSpearmanAttackUpRightAnimation;
    private bool _hasSpearmanAttackDownRightAnimation;
    private bool _useWarriorSecondAttack;
    private bool _isDead;

    public Animator Animator => _animator;
    public string IdleAnimationStateName => _idleAnimationStateName;
    public string RunAnimationStateName => _runAnimationStateName;
    public string BuildAnimationStateName => _buildAnimationStateName;
    public string AttackAnimationStateName => _attackAnimationStateName;
    public int AnimationLayerIndex => _animationLayerIndex;
    public float AnimationCrossFadeDuration => _animationCrossFadeDuration;

    public UnitType UnitType => _unitType;
    public int OwnerPlayerId => _ownerPlayerId;
    public TeamColor TeamColor => _teamColor;
    public float MoveSpeed => _moveSpeed;

    public UnitContext Context => _context;
    public UnitData Data => _config.Data;
    public int PlayerId => _playerId;
    public bool SelectableByLocalPlayer => _selectableByLocalPlayer;
    public bool IsAlive => !_isDead && _context != null && _context.CurrentHealth > 0f;
    public Vector3 Position => transform.position;
    public bool CanBeRepaired => Data.CanBeRepaired;
    public bool NeedsRepair => _context != null && _context.CurrentHealth < Data.MaxHealth;
    public bool CanAttack => IsAlive && Data.CanAttack && _unitType != UnitType.Worker && _unitType != UnitType.Monk;
    public GameObject DeathSmokePrefab => _deathSmokePrefab;
    public float DeathSmokeLifetime => _deathSmokeLifetime;

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

    public bool IsWorker()
    {
        return _unitType == UnitType.Worker;
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

    public bool IsEnemy(IAttackTarget target)
    {
        if (target == null)
            return false;

        return target.TeamColor != _teamColor;
    }

    public bool IsAlly(IAttackTarget target)
    {
        if (target == null)
            return false;

        return target.TeamColor == _teamColor;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        if (damage <= 0f)
            return;

        var finalDamage = Mathf.Max(damage - Data.Armor, 1f);
        _context.CurrentHealth -= finalDamage;

        if (_context.CurrentHealth <= 0f)
            Die();
    }

    public void Repair(float amount)
    {
        if (_isDead)
            return;

        _context.CurrentHealth = Mathf.Min(_context.CurrentHealth + amount, Data.MaxHealth);
    }

    public bool HasAnimationState(string stateName)
    {
        if (_animator == null)
            return false;

        if (string.IsNullOrWhiteSpace(stateName))
            return false;

        if (_animationLayerIndex < 0 || _animationLayerIndex >= _animator.layerCount)
            return false;

        int stateHash = Animator.StringToHash(stateName);
        return _animator.HasState(_animationLayerIndex, stateHash);
    }

    public bool IsCurrentAnimationState(string stateName)
    {
        if (_animator == null)
            return false;

        if (string.IsNullOrWhiteSpace(stateName))
            return false;

        if (_animationLayerIndex < 0 || _animationLayerIndex >= _animator.layerCount)
            return false;

        int stateHash = Animator.StringToHash(stateName);
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(_animationLayerIndex);
        AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(_animationLayerIndex);

        if (_animator.IsInTransition(_animationLayerIndex))
            return transitionInfo.fullPathHash == stateHash || transitionInfo.nameHash == stateHash;

        return stateInfo.shortNameHash == stateHash || stateInfo.fullPathHash == stateHash;
    }

    public void PlayAnimationState(string stateName)
    {
        if (_animator == null)
            return;

        if (string.IsNullOrWhiteSpace(stateName))
            return;

        if (!HasAnimationState(stateName))
            return;

        int stateHash = Animator.StringToHash(stateName);
        _animator.CrossFade(stateHash, _animationCrossFadeDuration, _animationLayerIndex);
    }

    public void PlayAnimationStateImmediate(string stateName)
    {
        if (_animator == null)
            return;

        if (string.IsNullOrWhiteSpace(stateName))
            return;

        if (!HasAnimationState(stateName))
            return;

        int stateHash = Animator.StringToHash(stateName);
        _animator.Play(stateHash, _animationLayerIndex, 0f);
    }

    public UnitAttackProfile PlayAttackAnimation(IAttackTarget target)
    {
        if (_animator == null)
            return UnitAttackProfile.Default(_defaultAttackDuration, _defaultAttackDuration * 0.5f);

        if (_unitType == UnitType.Warrior)
            return PlayWarriorAttackAnimation();

        if (_unitType == UnitType.Spearman)
            return PlaySpearmanAttackAnimation(target);

        if (_unitType == UnitType.Archer)
            return PlayArcherAttackAnimation();

        return PlayDefaultAttackAnimation();
    }

    public void SpawnArrow(Vector3 startPosition, Vector3 targetPosition, float damage)
    {
        if (_arrowPrefab == null)
            return;

        UpdateArrowSpawnPoint();

        var spawnPosition = _arrowSpawnPoint != null ? _arrowSpawnPoint.position : startPosition;
        var arrowObject = Instantiate(_arrowPrefab, spawnPosition, Quaternion.identity);
        var arrow = arrowObject.GetComponent<ArrowProjectile>();

        if (arrow == null)
            return;

        arrow.Initialize(this, TeamColor, spawnPosition, targetPosition, damage, Data.AttackRange, _deathSmokePrefab, _deathSmokeLifetime);
    }

    public void ForceRefreshAnimation()
    {
        if (_animator == null)
            return;

        if (_context == null)
            return;

        _lastMovingState = !_context.IsMoving;

        if (_context.IsMoving)
            PlayRunAnimation();
        else
            PlayIdleAnimation();

        _lastMovingState = _context.IsMoving;
    }

    private void Initialize()
    {
        _context = new UnitContext();
        _context.Initialize(this, _config.Data, transform, _playerId);

        _stateMachine = new UnitStateMachine();
        _brain = new UnitBrain();
        _brain.Initialize(_context, _stateMachine);

        _selectionMarker = GetComponent<UnitSelectionMarker>();
        _workerConstructionAgent = GetComponent<WorkerConstructionAgent>();

        _idleAnimationStateHash = Animator.StringToHash(_idleAnimationStateName);
        _runAnimationStateHash = Animator.StringToHash(_runAnimationStateName);
        _buildAnimationStateHash = Animator.StringToHash(_buildAnimationStateName);
        _attackAnimationStateHash = Animator.StringToHash(_attackAnimationStateName);
        _warriorAttackAnimationStateHashA = Animator.StringToHash(_warriorAttackAnimationStateNameA);
        _warriorAttackAnimationStateHashB = Animator.StringToHash(_warriorAttackAnimationStateNameB);
        _spearmanAttackRightAnimationStateHash = Animator.StringToHash(_spearmanAttackRightAnimationStateName);
        _spearmanAttackUpAnimationStateHash = Animator.StringToHash(_spearmanAttackUpAnimationStateName);
        _spearmanAttackDownAnimationStateHash = Animator.StringToHash(_spearmanAttackDownAnimationStateName);
        _spearmanAttackUpRightAnimationStateHash = Animator.StringToHash(_spearmanAttackUpRightAnimationStateName);
        _spearmanAttackDownRightAnimationStateHash = Animator.StringToHash(_spearmanAttackDownRightAnimationStateName);

        _hasIdleAnimation = HasAnimationState(_idleAnimationStateHash);
        _hasRunAnimation = HasAnimationState(_runAnimationStateHash);
        _hasBuildAnimation = HasAnimationState(_buildAnimationStateName);
        _hasAttackAnimation = HasAnimationState(_attackAnimationStateHash);
        _hasWarriorAttackAnimationA = HasAnimationState(_warriorAttackAnimationStateHashA);
        _hasWarriorAttackAnimationB = HasAnimationState(_warriorAttackAnimationStateHashB);
        _hasSpearmanAttackRightAnimation = HasAnimationState(_spearmanAttackRightAnimationStateHash);
        _hasSpearmanAttackUpAnimation = HasAnimationState(_spearmanAttackUpAnimationStateHash);
        _hasSpearmanAttackDownAnimation = HasAnimationState(_spearmanAttackDownAnimationStateHash);
        _hasSpearmanAttackUpRightAnimation = HasAnimationState(_spearmanAttackUpRightAnimationStateHash);
        _hasSpearmanAttackDownRightAnimation = HasAnimationState(_spearmanAttackDownRightAnimationStateHash);

        _lastMovingState = false;
        _useWarriorSecondAttack = false;
        _isDead = false;
        PlayIdleAnimationImmediate();
    }

    private void Tick(float deltaTime)
    {
        if (_isDead)
            return;

        if (_context.AttackCooldown > 0f)
            _context.AttackCooldown -= deltaTime;

        _brain.Update(deltaTime);
        UpdateVisualDirection();
        UpdateArrowSpawnPoint();
        UpdateAnimation();
    }
    private void UpdateArrowSpawnPoint()
    {
        if (_arrowSpawnPoint == null)
            return;

        if (_unitType != UnitType.Archer)
            return;

        var rightPosition = _arrowSpawnPointRightLocalPosition;
        var visualScaleX = _visualRoot != null ? _visualRoot.localScale.x : 1f;
        var lookingLeft = visualScaleX < 0f;

        var position = _arrowSpawnPoint.localPosition;
        position.x = lookingLeft ? -Mathf.Abs(rightPosition.x) : Mathf.Abs(rightPosition.x);
        position.y = rightPosition.y;
        _arrowSpawnPoint.localPosition = position;
    }
    private void UpdateVisualDirection()
    {
        if (_visualRoot == null)
            return;

        if (_context.IsMoving)
        {
            var directionX = _context.MoveDirection.x;

            if (Mathf.Abs(directionX) <= 0.001f)
                return;

            SetVisualDirection(directionX);
            return;
        }

        var attackTarget = _context.AttackTarget;

        if (attackTarget == null)
            return;

        if (!attackTarget.IsAlive)
        {
            _context.AttackTarget = null;
            return;
        }

        var attackDirectionX = attackTarget.Position.x - transform.position.x;

        if (Mathf.Abs(attackDirectionX) <= 0.001f)
            return;

        SetVisualDirection(attackDirectionX);
    }

    private void SetVisualDirection(float directionX)
    {
        var scale = _visualRoot.localScale;
        var absX = Mathf.Abs(scale.x);
        scale.x = directionX < 0f ? -absX : absX;
        _visualRoot.localScale = scale;
    }

    private void UpdateAnimation()
    {
        if (_animator == null)
            return;

        if (IsConstructionAnimationLocked())
            return;

        if (_context.IsAttackAnimationLocked)
            return;

        if (_context.IsMoving == _lastMovingState)
            return;

        _lastMovingState = _context.IsMoving;

        if (_context.IsMoving)
            PlayRunAnimation();
        else
            PlayIdleAnimation();
    }

    private bool IsConstructionAnimationLocked()
    {
        if (_workerConstructionAgent == null)
            return false;

        return _workerConstructionAgent.IsBuildingAnimationLocked;
    }

    private UnitAttackProfile PlayDefaultAttackAnimation()
    {
        if (_hasAttackAnimation)
            _animator.CrossFade(_attackAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);

        return UnitAttackProfile.Default(_defaultAttackDuration, _defaultAttackDuration * 0.5f);
    }

    private UnitAttackProfile PlayArcherAttackAnimation()
    {
        if (_hasAttackAnimation)
            _animator.CrossFade(_attackAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);

        return UnitAttackProfile.Projectile(_archerAttackDuration, _archerAttackDuration * 6f / 8f);
    }

    private UnitAttackProfile PlayWarriorAttackAnimation()
    {
        if (_useWarriorSecondAttack && _hasWarriorAttackAnimationB)
        {
            _animator.CrossFade(_warriorAttackAnimationStateHashB, _animationCrossFadeDuration, _animationLayerIndex);
            _useWarriorSecondAttack = false;
            return UnitAttackProfile.Default(_warriorAttackDuration, _warriorAttackDuration * 3f / 4f);
        }

        if (_hasWarriorAttackAnimationA)
        {
            _animator.CrossFade(_warriorAttackAnimationStateHashA, _animationCrossFadeDuration, _animationLayerIndex);
            _useWarriorSecondAttack = true;
            return UnitAttackProfile.Default(_warriorAttackDuration, _warriorAttackDuration * 3f / 4f);
        }

        if (_hasWarriorAttackAnimationB)
        {
            _animator.CrossFade(_warriorAttackAnimationStateHashB, _animationCrossFadeDuration, _animationLayerIndex);
            _useWarriorSecondAttack = false;
            return UnitAttackProfile.Default(_warriorAttackDuration, _warriorAttackDuration * 3f / 4f);
        }

        return PlayDefaultAttackAnimation();
    }

    private UnitAttackProfile PlaySpearmanAttackAnimation(IAttackTarget target)
    {
        if (target == null)
            return PlayDefaultAttackAnimation();

        var direction = target.Position - transform.position;
        direction.z = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return PlayDefaultAttackAnimation();

        var absX = Mathf.Abs(direction.x);
        var absY = Mathf.Abs(direction.y);
        var normalizedX = absX <= 0.001f ? 0f : 1f;
        var normalizedY = direction.y;

        if (absY > absX * 1.35f)
        {
            if (normalizedY > 0f && _hasSpearmanAttackUpAnimation)
            {
                _animator.CrossFade(_spearmanAttackUpAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
                return UnitAttackProfile.Default(_spearmanAttackDuration, _spearmanAttackDuration * 2f / 3f);
            }

            if (normalizedY < 0f && _hasSpearmanAttackDownAnimation)
            {
                _animator.CrossFade(_spearmanAttackDownAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
                return UnitAttackProfile.Default(_spearmanAttackDuration, _spearmanAttackDuration * 2f / 3f);
            }
        }

        if (absY > absX * 0.45f)
        {
            if (normalizedY > 0f && _hasSpearmanAttackUpRightAnimation)
            {
                _animator.CrossFade(_spearmanAttackUpRightAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
                return UnitAttackProfile.Default(_spearmanAttackDuration, _spearmanAttackDuration * 2f / 3f);
            }

            if (normalizedY < 0f && _hasSpearmanAttackDownRightAnimation)
            {
                _animator.CrossFade(_spearmanAttackDownRightAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
                return UnitAttackProfile.Default(_spearmanAttackDuration, _spearmanAttackDuration * 2f / 3f);
            }
        }

        if (normalizedX > 0f && _hasSpearmanAttackRightAnimation)
        {
            _animator.CrossFade(_spearmanAttackRightAnimationStateHash, _animationCrossFadeDuration, _animationLayerIndex);
            return UnitAttackProfile.Default(_spearmanAttackDuration, _spearmanAttackDuration * 2f / 3f);
        }

        return PlayDefaultAttackAnimation();
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

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        if (_deathSmokePrefab != null)
        {
            var smoke = Instantiate(_deathSmokePrefab, transform.position, Quaternion.identity);

            if (_deathSmokeLifetime > 0f)
                Destroy(smoke, _deathSmokeLifetime);
        }

        Destroy(gameObject);
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