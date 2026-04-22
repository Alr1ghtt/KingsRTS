using UnityEngine;

[RequireComponent(typeof(Unit))]
public class WorkerConstructionAgent : MonoBehaviour
{
    [SerializeField] private float _buildRange = 1.5f;

    private Unit _unit;
    private ConstructionSite _assignedSite;

    public Unit Unit => _unit;
    public ConstructionSite AssignedSite => _assignedSite;
    public float BuildRange => _buildRange;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public bool IsAssignedTo(ConstructionSite site)
    {
        return _assignedSite == site;
    }

    public void AssignToSite(ConstructionSite site)
    {
        if (site == null)
            return;

        if (_assignedSite == site)
            return;

        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        _assignedSite = site;

        if (!_assignedSite.AssignWorker(this))
        {
            _assignedSite = null;
        }
    }

    public void ClearAssignment()
    {
        if (_assignedSite != null)
            _assignedSite.RemoveWorker(this);

        _assignedSite = null;
    }

    public bool IsInBuildRange()
    {
        if (_assignedSite == null)
            return false;

        float distance = Vector3.Distance(transform.position, _assignedSite.transform.position);
        return distance <= _buildRange;
    }

    public void OnConstructionCompleted(ConstructionSite site)
    {
        if (_assignedSite != site)
            return;

        _assignedSite = null;
    }
}