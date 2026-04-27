using UnityEngine;

[RequireComponent(typeof(Unit))]
public class WorkerConstructionAgent : MonoBehaviour
{
    [SerializeField] private float _buildRange = 1.5f;
    [SerializeField] private float _pendingApproachArrivalDistance = 1.15f;
    [SerializeField] private float _leaveBuildRangeDistance = 2.25f;
    [SerializeField] private Vector2 _pendingBuildAreaSize = new Vector2(2.664385f, 2.664385f);
    [SerializeField] private Vector2 _pendingBuildAreaOffset = new Vector2(0.008683f, 0f);
    [SerializeField] private float _pendingBuildAreaPadding = 0.65f;
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private bool _forceBuildAnimationEveryFrame = true;

    private Unit _unit;
    private BuildingPlacementSystem _buildingPlacementSystem;
    private ConstructionSite _assignedSite;

    private BuildingData _pendingBuildingData;
    private Vector3 _pendingBuildPoint;
    private Vector3 _pendingApproachPoint;
    private int _pendingOwnerPlayerId;
    private TeamColor _pendingTeamColor;
    private bool _hasPendingBuild;
    private bool _isBuildingAnimationLocked;
    private bool _buildAnimationStarted;
    private bool _wasCancelled;

    public Unit Unit => _unit;
    public ConstructionSite AssignedSite => _assignedSite;
    public float BuildRange => _buildRange;
    public bool HasPendingBuild => _hasPendingBuild;
    public bool DisableLocalAvoidance => _assignedSite != null;
    public bool IsBuilding => _assignedSite != null && !_assignedSite.IsCompleted && !_wasCancelled;
    public bool IsActivelyBuilding => IsBuilding && IsInBuildRange();
    public bool IsBuildingAnimationLocked => _isBuildingAnimationLocked;
    public Vector3 PendingApproachPoint => _pendingApproachPoint;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        ResetConstructionState();
    }

    private void Update()
    {
        if (_wasCancelled)
        {
            StopBuildAnimationLock();
            return;
        }

        if (_hasPendingBuild && _assignedSite != null)
        {
            Log($"Рабочий {name}: конфликтное состояние, сброс");
            ResetConstructionState();
            StopBuildAnimationLock();
            return;
        }

        if (_hasPendingBuild)
        {
            UpdatePendingBuild();
            return;
        }

        if (_assignedSite == null)
        {
            StopBuildAnimationLock();
            return;
        }

        if (_assignedSite.IsCompleted)
        {
            Log($"Рабочий {name}: стройка завершена, очистка");
            ResetAssignedSiteOnly();
            StopBuildAnimationLock();
            return;
        }

        if (IsInBuildRange())
        {
            HoldBuildAnimation();
            return;
        }

        if (!IsFarFromAssignedSite())
        {
            HoldBuildAnimation();
            return;
        }

        StopBuildAnimationLock();
    }

    public bool IsAssignedTo(ConstructionSite site)
    {
        return _assignedSite == site;
    }

    public void StartPendingBuild(BuildingData buildingData, Vector3 buildPoint, Vector3 approachPoint, int ownerPlayerId, TeamColor teamColor, BuildingPlacementSystem placementSystem)
    {
        if (buildingData == null)
            return;

        ResetConstructionState();
        StopBuildAnimationLock();

        _buildingPlacementSystem = placementSystem;
        _pendingBuildingData = buildingData;
        _pendingBuildPoint = buildPoint;
        _pendingBuildPoint.z = transform.position.z;
        _pendingApproachPoint = approachPoint;
        _pendingApproachPoint.z = transform.position.z;
        _pendingOwnerPlayerId = ownerPlayerId;
        _pendingTeamColor = teamColor;
        _hasPendingBuild = true;
        _wasCancelled = false;

        Log($"Рабочий {name}: получил приказ строить {_pendingBuildingData.DisplayName} в точке {_pendingBuildPoint}, точка подхода {_pendingApproachPoint}");
    }

    public void AssignToSite(ConstructionSite site)
    {
        if (site == null)
            return;

        ResetPendingBuildOnly();

        if (_assignedSite == site)
        {
            Log($"Рабочий {name}: уже назначен на эту стройку");
            return;
        }

        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        _assignedSite = site;
        _wasCancelled = false;

        if (!_assignedSite.AssignWorker(this))
        {
            Log($"Рабочий {name}: не удалось назначиться на стройку");
            ResetAssignedSiteOnly();
            StopBuildAnimationLock();
            return;
        }

        Log($"Рабочий {name}: назначен на стройку {_assignedSite.BuildingData.DisplayName}");
    }

    public void ClearAssignment()
    {
        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        ResetAssignedSiteOnly();
        StopBuildAnimationLock();
    }

    public void CancelConstructionOrder()
    {
        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        ResetConstructionState();
        StopBuildAnimationLock();

        _wasCancelled = true;

        Log($"Рабочий {name}: приказ на строительство отменен");
    }

    public bool IsInBuildRange()
    {
        if (_assignedSite == null)
            return false;

        return _assignedSite.IsPointInBuildPerimeter(transform.position);
    }

    public void OnConstructionCompleted(ConstructionSite site)
    {
        if (_assignedSite != site)
            return;

        Log($"Рабочий {name}: строительство завершено");

        ResetConstructionState();
        StopBuildAnimationLock();

        if (_unit != null)
            _unit.ForceRefreshAnimation();
    }

    private void UpdatePendingBuild()
    {
        if (!CanCreatePendingConstructionSite())
            return;

        if (_buildingPlacementSystem == null)
        {
            Log($"Рабочий {name}: отсутствует BuildingPlacementSystem");
            ResetConstructionState();
            StopBuildAnimationLock();
            return;
        }

        if (!_buildingPlacementSystem.TryPlaceBuilding(_pendingBuildingData, _pendingBuildPoint, _pendingOwnerPlayerId, _pendingTeamColor, out ConstructionSite site))
        {
            Log($"Рабочий {name}: не удалось создать стройплощадку {_pendingBuildingData.DisplayName}");
            ResetConstructionState();
            StopBuildAnimationLock();
            return;
        }

        Log($"Рабочий {name}: прибыл к периметру, создана стройплощадка {_pendingBuildingData.DisplayName}");

        ResetPendingBuildOnly();
        AssignToSite(site);
    }

    private bool CanCreatePendingConstructionSite()
    {
        if (IsInPendingBuildPerimeter(transform.position))
            return true;

        float distanceToApproachPoint = Vector3.Distance(transform.position, _pendingApproachPoint);
        return distanceToApproachPoint <= _pendingApproachArrivalDistance;
    }

    private bool IsInPendingBuildPerimeter(Vector3 point)
    {
        Vector3 center = _pendingBuildPoint + new Vector3(_pendingBuildAreaOffset.x, _pendingBuildAreaOffset.y, 0f);

        float halfWidth = _pendingBuildAreaSize.x * 0.5f + _pendingBuildAreaPadding;
        float halfHeight = _pendingBuildAreaSize.y * 0.5f + _pendingBuildAreaPadding;

        return point.x >= center.x - halfWidth &&
               point.x <= center.x + halfWidth &&
               point.y >= center.y - halfHeight &&
               point.y <= center.y + halfHeight;
    }

    private bool IsFarFromAssignedSite()
    {
        if (_assignedSite == null)
            return true;

        float distance = Vector3.Distance(transform.position, _assignedSite.transform.position);
        return distance > _leaveBuildRangeDistance;
    }

    private void HoldBuildAnimation()
    {
        _isBuildingAnimationLocked = true;

        if (_unit == null)
            return;

        if (_unit.Animator == null)
            return;

        if (string.IsNullOrWhiteSpace(_unit.BuildAnimationStateName))
            return;

        if (!_unit.HasAnimationState(_unit.BuildAnimationStateName))
        {
            Log($"Рабочий {name}: в Animator нет состояния '{_unit.BuildAnimationStateName}'");
            return;
        }

        if (!_buildAnimationStarted)
        {
            _unit.PlayAnimationStateImmediate(_unit.BuildAnimationStateName);
            _buildAnimationStarted = true;
            Log($"Рабочий {name}: включена анимация строительства");
            return;
        }

        if (_forceBuildAnimationEveryFrame)
        {
            if (!_unit.IsCurrentAnimationState(_unit.BuildAnimationStateName))
                _unit.PlayAnimationStateImmediate(_unit.BuildAnimationStateName);

            return;
        }

        if (!_unit.IsCurrentAnimationState(_unit.BuildAnimationStateName))
            _unit.PlayAnimationState(_unit.BuildAnimationStateName);
    }

    private void StopBuildAnimationLock()
    {
        if (!_isBuildingAnimationLocked && !_buildAnimationStarted)
            return;

        _isBuildingAnimationLocked = false;
        _buildAnimationStarted = false;

        if (_unit != null)
            _unit.ForceRefreshAnimation();
    }

    private void ResetConstructionState()
    {
        _hasPendingBuild = false;
        _pendingBuildingData = null;
        _pendingBuildPoint = Vector3.zero;
        _pendingApproachPoint = Vector3.zero;
        _pendingOwnerPlayerId = 0;
        _pendingTeamColor = default;
        _assignedSite = null;
    }

    private void ResetPendingBuildOnly()
    {
        _hasPendingBuild = false;
        _pendingBuildingData = null;
        _pendingBuildPoint = Vector3.zero;
        _pendingApproachPoint = Vector3.zero;
        _pendingOwnerPlayerId = 0;
        _pendingTeamColor = default;
    }

    private void ResetAssignedSiteOnly()
    {
        _assignedSite = null;
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log(message, this);
    }
}