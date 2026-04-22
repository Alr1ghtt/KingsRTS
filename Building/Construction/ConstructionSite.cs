using System.Collections.Generic;
using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    [SerializeField] private BuildingData _buildingData;
    [SerializeField] private int _ownerPlayerId;
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private float _currentProgress;
    [SerializeField] private bool _isCompleted;

    private readonly List<ConstructionWorkerSlot> _workers = new List<ConstructionWorkerSlot>();

    public BuildingData BuildingData => _buildingData;
    public int OwnerPlayerId => _ownerPlayerId;
    public TeamColor TeamColor => _teamColor;
    public float CurrentProgress => _currentProgress;
    public float BuildTime => _buildingData != null ? _buildingData.BuildTime : 0f;
    public bool IsCompleted => _isCompleted;
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
    }

    public bool CanAssignWorker(WorkerConstructionAgent worker)
    {
        if (_isCompleted)
            return false;

        if (worker == null)
            return false;

        if (!worker.Unit.IsWorker())
            return false;

        if (worker.Unit.OwnerPlayerId != _ownerPlayerId)
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
        return true;
    }

    public void RemoveWorker(WorkerConstructionAgent worker)
    {
        if (worker == null)
            return;

        for (int i = _workers.Count - 1; i >= 0; i--)
        {
            if (_workers[i].Worker == worker)
                _workers.RemoveAt(i);
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

        for (int i = 0; i < _workers.Count; i++)
        {
            if (_workers[i].Worker != null)
                _workers[i].Worker.OnConstructionCompleted(this);
        }

        if (_buildingData == null || _buildingData.BuildingPrefab == null)
            return;

        Building building = Instantiate(_buildingData.BuildingPrefab, transform.position, transform.rotation);
        building.Initialize(_buildingData, _ownerPlayerId, _teamColor);

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
            {
                _workers.RemoveAt(i);
            }
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
}