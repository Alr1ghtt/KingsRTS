using UnityEngine;

public class UnitSelectionMarker : MonoBehaviour
{
    [SerializeField] private GameObject _selectionVisual;

    private void Awake()
    {
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (_selectionVisual != null)
            _selectionVisual.SetActive(selected);
    }
}