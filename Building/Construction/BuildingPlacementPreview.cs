using UnityEngine;

public class BuildingPlacementPreview : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    public void Show(BuildingData buildingData, Vector3 worldPosition)
    {
        if (_renderer == null)
        {
            Debug.Log("Preview: renderer отсутствует", this);
            return;
        }

        if (buildingData == null)
        {
            Debug.Log("Preview: buildingData = null", this);
            Hide();
            return;
        }

        if (buildingData.PreviewSprite == null)
        {
            Debug.Log($"Preview: у {buildingData.DisplayName} не задан PreviewSprite", this);
            Hide();
            return;
        }

        _renderer.sprite = buildingData.PreviewSprite;
        _renderer.color = new Color(1f, 1f, 1f, 0.25f);
        _renderer.sortingOrder = 1000;

        Vector3 position = worldPosition;
        position.z = 0f;
        transform.position = position;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Debug.Log($"Preview: показан {buildingData.DisplayName}, sprite = {_renderer.sprite.name}, position = {transform.position}", this);
    }

    public void Hide()
    {
        if (_renderer != null)
            _renderer.sprite = null;

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}