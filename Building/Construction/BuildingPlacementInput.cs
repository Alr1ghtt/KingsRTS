using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementInput : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private BuildingPlacementSystem _placementSystem;

    [Header("Buildings")]
    [SerializeField] private BuildingData _archeryData;
    [SerializeField] private BuildingData _barracksData;
    [SerializeField] private BuildingData _castleData;
    [SerializeField] private BuildingData _houseData;
    [SerializeField] private BuildingData _monasteryData;
    [SerializeField] private BuildingData _towerData;

    private BuildingPlacementState _state;
    private ConstructionCommand _currentCommand;
    private readonly List<WorkerConstructionAgent> _selectedWorkers = new List<WorkerConstructionAgent>();

    public void SetSelectedWorkers(List<WorkerConstructionAgent> workers)
    {
        _selectedWorkers.Clear();

        if (workers == null)
            return;

        for (int i = 0; i < workers.Count; i++)
        {
            if (workers[i] == null)
                continue;

            if (!workers[i].Unit.IsWorker())
                continue;

            _selectedWorkers.Add(workers[i]);
        }
    }

    private void Update()
    {
        HandleKeyboard();
        HandlePlacementClick();
    }

    private void HandleKeyboard()
    {
        if (_selectedWorkers.Count == 0)
        {
            _state = BuildingPlacementState.None;
            _currentCommand = null;
            return;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_state == BuildingPlacementState.None)
            {
                _state = BuildingPlacementState.WaitingForBuildingType;
                _currentCommand = null;
                return;
            }

            if (_state == BuildingPlacementState.WaitingForBuildingType)
            {
                SelectBuilding(_barracksData);
            }
        }

        if (_state != BuildingPlacementState.WaitingForBuildingType)
            return;

        if (Input.GetKeyDown(KeyCode.A))
            SelectBuilding(_archeryData);

        if (Input.GetKeyDown(KeyCode.C))
            SelectBuilding(_castleData);

        if (Input.GetKeyDown(KeyCode.H))
            SelectBuilding(_houseData);

        if (Input.GetKeyDown(KeyCode.M))
            SelectBuilding(_monasteryData);

        if (Input.GetKeyDown(KeyCode.T))
            SelectBuilding(_towerData);

        if (Input.GetMouseButtonDown(1))
        {
            _state = BuildingPlacementState.None;
            _currentCommand = null;
        }
    }

    private void HandlePlacementClick()
    {
        if (_currentCommand == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (!plane.Raycast(ray, out float enter))
            return;

        Vector3 worldPoint = ray.GetPoint(enter);

        WorkerConstructionAgent firstWorker = GetFirstWorker();
        if (firstWorker == null)
            return;

        if (!_placementSystem.TryPlaceBuilding(_currentCommand.BuildingData, worldPoint, firstWorker.Unit.OwnerPlayerId, firstWorker.Unit.TeamColor, out ConstructionSite site))
            return;

        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] == null)
                continue;

            _selectedWorkers[i].AssignToSite(site);
        }

        _state = BuildingPlacementState.None;
        _currentCommand = null;
    }

    private void SelectBuilding(BuildingData buildingData)
    {
        if (buildingData == null)
            return;

        _currentCommand = new ConstructionCommand(buildingData);
    }

    private WorkerConstructionAgent GetFirstWorker()
    {
        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] != null)
                return _selectedWorkers[i];
        }

        return null;
    }
}