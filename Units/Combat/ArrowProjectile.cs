using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _arcHeight = 0.7f;
    [SerializeField] private float _hitRadius = 0.2f;
    [SerializeField] private float _missLifeTime = 2f;
    [SerializeField] private float _overshootDistance = 0.35f;

    private Unit _owner;
    private TeamColor _teamColor;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Vector3 _previousPosition;
    private float _damage;
    private float _travelTime;
    private float _timer;
    private bool _stopped;
    private GameObject _deathSmokePrefab;
    private float _deathSmokeLifetime;

    public void Initialize(Unit owner, TeamColor teamColor, Vector3 startPosition, Vector3 targetPosition, float damage, float maxRange, GameObject deathSmokePrefab, float deathSmokeLifetime)
    {
        _owner = owner;
        _teamColor = teamColor;
        _startPosition = startPosition;
        _previousPosition = startPosition;
        _damage = damage;
        _deathSmokePrefab = deathSmokePrefab;
        _deathSmokeLifetime = deathSmokeLifetime;

        var direction = targetPosition - startPosition;
        direction.z = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector3.right;

        var distance = Mathf.Min(direction.magnitude + _overshootDistance, maxRange + _overshootDistance);
        _endPosition = startPosition + direction.normalized * distance;
        _travelTime = Mathf.Max(0.05f, distance / Mathf.Max(0.01f, _speed));
        _timer = 0f;
        _stopped = false;
        transform.position = startPosition;
        RotateToDirection(direction);
    }

    private void Update()
    {
        if (_stopped)
            return;

        _timer += Time.deltaTime;

        var t = Mathf.Clamp01(_timer / _travelTime);
        var nextPosition = GetArcPosition(t);
        var moveDirection = nextPosition - _previousPosition;

        transform.position = nextPosition;

        if (moveDirection.sqrMagnitude > 0.0001f)
            RotateToDirection(moveDirection);

        if (TryHitEnemy())
            return;

        _previousPosition = nextPosition;

        if (t >= 1f)
            StopAsMiss();
    }

    private Vector3 GetArcPosition(float t)
    {
        var position = Vector3.Lerp(_startPosition, _endPosition, t);
        position.y += Mathf.Sin(t * Mathf.PI) * _arcHeight;
        return position;
    }

    private bool TryHitEnemy()
    {
        var units = UnitRegistry.Units;

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];

            if (unit == null)
                continue;

            if (unit == _owner)
                continue;

            if (!unit.IsAlive)
                continue;

            if (unit.TeamColor == _teamColor)
                continue;

            var distance = Vector3.Distance(transform.position, unit.Position);

            if (distance > _hitRadius + unit.Data.Radius)
                continue;

            unit.TakeDamage(_damage);
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    private void RotateToDirection(Vector3 direction)
    {
        direction.z = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void StopAsMiss()
    {
        _stopped = true;

        if (_deathSmokePrefab != null)
        {
            var smoke = Instantiate(_deathSmokePrefab, transform.position, Quaternion.identity);

            if (_deathSmokeLifetime > 0f)
                Destroy(smoke, _deathSmokeLifetime);
        }

        Destroy(gameObject, _missLifeTime);
    }
}