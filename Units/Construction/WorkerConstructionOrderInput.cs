using System.Collections.Generic;
using UnityEngine;

public class WorkerConstructionOrderInput : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private bool _enableDebugLogs = true;

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
        if (_selectedWorkers.Count == 0)
            return;

        if (!Input.GetMouseButtonDown(1))
            return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (!hit.collider)
            return;

        ConstructionSite site = hit.collider.GetComponentInParent<ConstructionSite>();
        if (site == null)
            return;

        Log($"Назначение рабочих на существующую стройку: {site.BuildingData.DisplayName}");

        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] == null)
                continue;

            _selectedWorkers[i].AssignToSite(site);
        }
    }

    private void Log(string message)
    {
        if (_enableDebugLogs)
            Debug.Log(message, this);
    }
}