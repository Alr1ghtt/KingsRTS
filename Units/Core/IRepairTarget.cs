using UnityEngine;

public interface IRepairTarget
{
    int PlayerId => 0;
    bool IsAlive => false;
    bool CanBeRepaired => false;
    bool NeedsRepair => false;
    Vector3 Position => Vector3.zero;
    void Repair(float amount);
}