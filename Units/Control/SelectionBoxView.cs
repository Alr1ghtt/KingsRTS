using UnityEngine;

public class SelectionBoxView : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _box;

    private RectTransform _canvasRectTransform;

    private void Awake()
    {
        if (_canvas != null)
            _canvasRectTransform = _canvas.transform as RectTransform;

        if (_box != null)
        {
            _box.anchorMin = new Vector2(0f, 1f);
            _box.anchorMax = new Vector2(0f, 1f);
            _box.pivot = new Vector2(0f, 1f);
        }

        Hide();
    }

    public void Show()
    {
        if (_box != null)
            _box.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_box != null)
            _box.gameObject.SetActive(false);
    }

    public void UpdateBox(Vector2 startScreenPosition, Vector2 endScreenPosition)
    {
        if (_box == null)
            return;

        if (_canvasRectTransform == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, startScreenPosition, _canvas.worldCamera, out var localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, endScreenPosition, _canvas.worldCamera, out var localEnd);

        var xMin = Mathf.Min(localStart.x, localEnd.x);
        var xMax = Mathf.Max(localStart.x, localEnd.x);
        var yMin = Mathf.Min(localStart.y, localEnd.y);
        var yMax = Mathf.Max(localStart.y, localEnd.y);

        var canvasWidth = _canvasRectTransform.rect.width;
        var canvasHeight = _canvasRectTransform.rect.height;

        var anchoredX = xMin + canvasWidth * 0.5f;
        var anchoredY = yMax - canvasHeight * 0.5f;

        _box.anchoredPosition = new Vector2(anchoredX, anchoredY);
        _box.sizeDelta = new Vector2(xMax - xMin, yMax - yMin);
    }
}