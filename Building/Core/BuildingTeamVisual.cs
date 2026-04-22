using UnityEngine;

public class BuildingTeamVisual : MonoBehaviour
{
    [SerializeField] private GameObject _blackRoot;
    [SerializeField] private GameObject _blueRoot;
    [SerializeField] private GameObject _purpleRoot;
    [SerializeField] private GameObject _redRoot;
    [SerializeField] private GameObject _yellowRoot;

    public void Apply(TeamColor teamColor)
    {
        SetAllInactive();

        switch (teamColor)
        {
            case TeamColor.Black:
                SetActive(_blackRoot);
                break;
            case TeamColor.Blue:
                SetActive(_blueRoot);
                break;
            case TeamColor.Purple:
                SetActive(_purpleRoot);
                break;
            case TeamColor.Red:
                SetActive(_redRoot);
                break;
            case TeamColor.Yellow:
                SetActive(_yellowRoot);
                break;
        }
    }

    private void SetAllInactive()
    {
        SetInactive(_blackRoot);
        SetInactive(_blueRoot);
        SetInactive(_purpleRoot);
        SetInactive(_redRoot);
        SetInactive(_yellowRoot);
    }

    private void SetActive(GameObject target)
    {
        if (target != null)
            target.SetActive(true);
    }

    private void SetInactive(GameObject target)
    {
        if (target != null)
            target.SetActive(false);
    }
}