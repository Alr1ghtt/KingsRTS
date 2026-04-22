using UnityEngine;

[System.Serializable]
public class ConstructionWorkerSlot
{
    [SerializeField] private WorkerConstructionAgent _worker;

    public WorkerConstructionAgent Worker => _worker;

    public ConstructionWorkerSlot(WorkerConstructionAgent worker)
    {
        _worker = worker;
    }

    public bool IsValid()
    {
        return _worker != null;
    }
}