using System.Collections.Generic;
using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    [SerializeField] private BuildingData _buildingData;
    [SerializeField] private int _ownerPlayerId;
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private float _currentProgress;
    [SerializeField] private bool _isCompleted;
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private Transform _workPoint;

    private readonly List<ConstructionWorkerSlot> _workers = new List<ConstructionWorkerSlot>();

    public BuildingData BuildingData => _buildingData;
    public int OwnerPlayerId => _ownerPlayerId;
    public TeamColor TeamColor => _teamColor;
    public float CurrentProgress => _currentProgress;
    public float BuildTime => _buildingData != null ? _buildingData.BuildTime : 0f;
    public bool IsCompleted => _isCompleted;
    public Vector3 WorkPoint => transform.position;
    public int ActiveWorkersCount => GetActiveWorkersCount();
    public float NormalizedProgress => _buildingData == null || _buildingData.BuildTime <= 0f ? 0f : Mathf.Clamp01(_currentProgress / _buildingData.BuildTime);

    public void Initialize(BuildingData buildingData, int ownerPlayerId, TeamColor teamColor)
    {
        _buildingData = buildingData;
        _ownerPlayerId = ownerPlayerId;
        _teamColor = teamColor;
        _currentProgress = 0f;
        _isCompleted = false;
        _workers.Clear();

        Log($"Создана стройплощадка {_buildingData.DisplayName}, игрок {_ownerPlayerId}, команда {_teamColor}");
    }

    public bool CanAssignWorker(WorkerConstructionAgent worker)
    {
        if (_isCompleted)
            return false;

        if (worker == null)
            return false;

        if (worker.Unit.PlayerId != _ownerPlayerId)
            return false;

        for (int i = 0; i < _workers.Count; i++)
        {
            if (_workers[i].Worker == worker)
                return false;
        }

        return true;
    }

    public bool AssignWorker(WorkerConstructionAgent worker)
    {
        if (!CanAssignWorker(worker))
            return false;

        _workers.Add(new ConstructionWorkerSlot(worker));
        Log($"Рабочий {worker.name} добавлен на стройку {_buildingData.DisplayName}");
        return true;
    }

    public void RemoveWorker(WorkerConstructionAgent worker)
    {
        if (worker == null)
            return;

        for (int i = _workers.Count - 1; i >= 0; i--)
        {
            if (_workers[i].Worker == worker)
            {
                _workers.RemoveAt(i);
                Log($"Рабочий {worker.name} снят со стройки {_buildingData.DisplayName}");
            }
        }
    }

    public void Tick(float deltaTime)
    {
        if (_isCompleted)
            return;

        CleanupWorkers();

        int activeWorkers = GetActiveWorkersCount();

        if (activeWorkers <= 0)
            return;

        _currentProgress += activeWorkers * deltaTime;

        if (_currentProgress >= _buildingData.BuildTime)
        {
            _currentProgress = _buildingData.BuildTime;
            CompleteConstruction();
        }
    }

    private void CompleteConstruction()
    {
        if (_isCompleted)
            return;

        _isCompleted = true;

        Log($"Строительство завершено: {_buildingData.DisplayName}");

        for (int i = 0; i < _workers.Count; i++)
        {
            if (_workers[i].Worker != null)
                _workers[i].Worker.OnConstructionCompleted(this);
        }

        if (_buildingData == null)
        {
            Log("BuildingData отсутствует, здание не может быть создано");
            return;
        }

        if (_buildingData.BuildingPrefab == null)
        {
            Log($"У {_buildingData.DisplayName} не назначен BuildingPrefab");
            return;
        }

        Building building = Instantiate(_buildingData.BuildingPrefab, transform.position, transform.rotation);
        building.Initialize(_buildingData, _ownerPlayerId, _teamColor);

        Log($"Создано здание {_buildingData.DisplayName}, игрок {_ownerPlayerId}, команда {_teamColor}");

        Destroy(gameObject);
    }

    private void CleanupWorkers()
    {
        for (int i = _workers.Count - 1; i >= 0; i--)
        {
            WorkerConstructionAgent worker = _workers[i].Worker;

            if (worker == null)
            {
                _workers.RemoveAt(i);
                continue;
            }

            if (!worker.IsAssignedTo(this))
                _workers.RemoveAt(i);
        }
    }

    private int GetActiveWorkersCount()
    {
        int count = 0;

        for (int i = 0; i < _workers.Count; i++)
        {
            WorkerConstructionAgent worker = _workers[i].Worker;

            if (worker == null)
                continue;

            if (!worker.IsAssignedTo(this))
                continue;

            if (!worker.IsInBuildRange())
                continue;

            count++;
        }

        return count;
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log(message, this);
    }
}