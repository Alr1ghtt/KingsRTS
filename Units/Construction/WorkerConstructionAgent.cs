using UnityEngine;

[RequireComponent(typeof(Unit))]
public class WorkerConstructionAgent : MonoBehaviour
{
    [SerializeField] private float _buildRange = 1.5f;
    [SerializeField] private bool _enableDebugLogs = true;

    private enum WorkerAnimationState
    {
        None,
        Idle,
        Run,
        Build
    }

    private Unit _unit;
    private BuildingPlacementSystem _buildingPlacementSystem;
    private ConstructionSite _assignedSite;

    private BuildingData _pendingBuildingData;
    private Vector3 _pendingBuildPoint;
    private int _pendingOwnerPlayerId;
    private TeamColor _pendingTeamColor;
    private bool _hasPendingBuild;

    private WorkerAnimationState _currentAnimationState;

    public Unit Unit => _unit;
    public ConstructionSite AssignedSite => _assignedSite;
    public float BuildRange => _buildRange;
    public bool HasPendingBuild => _hasPendingBuild;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        ResetConstructionState();
        SetIdleAnimationIfNeeded();
    }

    private void Update()
    {
        if (_hasPendingBuild && _assignedSite != null)
        {
            Log($"Рабочий {name}: конфликтное состояние, сброс");
            ResetConstructionState();
            SetIdleAnimationIfNeeded();
            return;
        }

        if (_hasPendingBuild)
        {
            UpdatePendingBuild();
            return;
        }

        if (_assignedSite == null)
        {
            SetIdleAnimationIfNeeded();
            return;
        }

        if (_assignedSite.IsCompleted)
        {
            Log($"Рабочий {name}: стройка завершена, очистка");
            ResetAssignedSiteOnly();
            SetIdleAnimationIfNeeded();
            return;
        }

        if (IsInBuildRange())
        {
            SetBuildAnimationIfNeeded();
            return;
        }

        SetRunAnimationIfNeeded();
    }

    public bool IsAssignedTo(ConstructionSite site)
    {
        return _assignedSite == site;
    }

    public void StartPendingBuild(BuildingData buildingData, Vector3 buildPoint, int ownerPlayerId, TeamColor teamColor, BuildingPlacementSystem placementSystem)
    {
        if (buildingData == null)
            return;

        ResetConstructionState();

        _buildingPlacementSystem = placementSystem;
        _pendingBuildingData = buildingData;
        _pendingBuildPoint = buildPoint;
        _pendingBuildPoint.z = transform.position.z;
        _pendingOwnerPlayerId = ownerPlayerId;
        _pendingTeamColor = teamColor;
        _hasPendingBuild = true;

        SetRunAnimationIfNeeded();
        Log($"Рабочий {name}: получил приказ строить {_pendingBuildingData.DisplayName} в точке {_pendingBuildPoint}");
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

        if (!_assignedSite.AssignWorker(this))
        {
            Log($"Рабочий {name}: не удалось назначиться на стройку");
            ResetAssignedSiteOnly();
            SetIdleAnimationIfNeeded();
            return;
        }

        Log($"Рабочий {name}: назначен на стройку {_assignedSite.BuildingData.DisplayName}");
    }

    public void ClearAssignment()
    {
        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        ResetAssignedSiteOnly();
        SetIdleAnimationIfNeeded();
    }

    public void CancelConstructionOrder()
    {
        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        ResetConstructionState();
        SetIdleAnimationIfNeeded();
        Log($"Рабочий {name}: приказ на строительство отменен");
    }

    public bool IsInBuildRange()
    {
        if (_assignedSite == null)
            return false;

        float distance = Vector3.Distance(transform.position, _assignedSite.WorkPoint);
        return distance <= _buildRange;
    }

    public void OnConstructionCompleted(ConstructionSite site)
    {
        if (_assignedSite != site)
            return;

        Log($"Рабочий {name}: строительство завершено");

        _assignedSite = null;
        _hasPendingBuild = false;
        _pendingBuildingData = null;
        _pendingBuildPoint = Vector3.zero;

        SetIdleAnimationIfNeeded();
    }

    private void UpdatePendingBuild()
    {
        float distanceToBuildPoint = Vector3.Distance(transform.position, _pendingBuildPoint);

        if (distanceToBuildPoint > _buildRange)
        {
            SetRunAnimationIfNeeded();
            return;
        }

        if (_buildingPlacementSystem == null)
        {
            Log($"Рабочий {name}: отсутствует BuildingPlacementSystem");
            ResetConstructionState();
            SetIdleAnimationIfNeeded();
            return;
        }

        if (!_buildingPlacementSystem.TryPlaceBuilding(_pendingBuildingData, _pendingBuildPoint, _pendingOwnerPlayerId, _pendingTeamColor, out ConstructionSite site))
        {
            Log($"Рабочий {name}: не удалось создать стройплощадку {_pendingBuildingData.DisplayName}");
            ResetConstructionState();
            SetIdleAnimationIfNeeded();
            return;
        }

        Log($"Рабочий {name}: прибыл на точку, создана стройплощадка {_pendingBuildingData.DisplayName}");

        ResetPendingBuildOnly();
        AssignToSite(site);
    }

    private void ResetConstructionState()
    {
        _hasPendingBuild = false;
        _pendingBuildingData = null;
        _pendingBuildPoint = Vector3.zero;
        _pendingOwnerPlayerId = 0;
        _pendingTeamColor = default;
        _assignedSite = null;
    }

    private void ResetPendingBuildOnly()
    {
        _hasPendingBuild = false;
        _pendingBuildingData = null;
        _pendingBuildPoint = Vector3.zero;
        _pendingOwnerPlayerId = 0;
        _pendingTeamColor = default;
    }

    private void ResetAssignedSiteOnly()
    {
        _assignedSite = null;
    }

    private void SetIdleAnimationIfNeeded()
    {
        if (_currentAnimationState == WorkerAnimationState.Idle)
            return;

        PlayAnimation(_unit.IdleAnimationStateName, WorkerAnimationState.Idle);
    }

    private void SetRunAnimationIfNeeded()
    {
        if (_currentAnimationState == WorkerAnimationState.Run)
            return;

        PlayAnimation(_unit.RunAnimationStateName, WorkerAnimationState.Run);
    }

    private void SetBuildAnimationIfNeeded()
    {
        if (_currentAnimationState == WorkerAnimationState.Build)
            return;

        PlayAnimation(_unit.BuildAnimationStateName, WorkerAnimationState.Build);
        Log($"Рабочий {name}: начал строить");
    }

    private void PlayAnimation(string stateName, WorkerAnimationState state)
    {
        if (_unit.Animator == null)
            return;

        if (string.IsNullOrWhiteSpace(stateName))
            return;

        if (!HasState(0, stateName))
        {
            Log($"Рабочий {name}: в Animator нет состояния '{stateName}'");
            return;
        }

        _unit.Animator.CrossFade(stateName, 0.05f, 0);
        _currentAnimationState = state;
    }

    private bool HasState(int layerIndex, string stateName)
    {
        if (_unit.Animator == null)
            return false;

        int stateHash = Animator.StringToHash(stateName);
        return _unit.Animator.HasState(layerIndex, stateHash);
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log(message, this);
    }
}