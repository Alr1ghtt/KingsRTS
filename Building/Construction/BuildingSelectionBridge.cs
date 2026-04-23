using System.Collections.Generic;
using UnityEngine;

public class BuildingSelectionBridge : MonoBehaviour
{
    [SerializeField] private PlayerUnitController _playerUnitController;
    [SerializeField] private BuildingPlacementInput _buildingPlacementInput;
    [SerializeField] private WorkerConstructionOrderInput _workerConstructionOrderInput;
    [SerializeField] private bool _enableDebugLogs = true;

    private readonly List<WorkerConstructionAgent> _selectedWorkers = new List<WorkerConstructionAgent>();

    private void Update()
    {
        RefreshSelectedWorkers();
        PushSelectedWorkers();
    }

    private void RefreshSelectedWorkers()
    {
        _selectedWorkers.Clear();

        if (_playerUnitController == null)
            return;

        List<Unit> selectedUnits = _playerUnitController.SelectedUnits;
        if (selectedUnits == null)
            return;

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            Unit unit = selectedUnits[i];

            if (unit == null)
                continue;

            WorkerConstructionAgent worker = unit.GetComponent<WorkerConstructionAgent>();
            if (worker == null)
                continue;

            if (!worker.Unit.IsWorker())
                continue;

            _selectedWorkers.Add(worker);
        }
    }

    private void PushSelectedWorkers()
    {
        if (_buildingPlacementInput != null)
            _buildingPlacementInput.SetSelectedWorkers(_selectedWorkers);

        if (_workerConstructionOrderInput != null)
            _workerConstructionOrderInput.SetSelectedWorkers(_selectedWorkers);
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log(message, this);
    }
}