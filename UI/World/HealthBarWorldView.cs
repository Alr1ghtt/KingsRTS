using UnityEngine;
using UnityEngine.UI;

public class HealthBarWorldView : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private bool _hideWhenFull = true;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private bool _faceCamera = true;

    private IHealthViewTarget _target;

    public void Initialize(IHealthViewTarget target)
    {
        _target = target;
        UpdateView();
    }

    private void LateUpdate()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        if (_target == null || !_target.IsAlive)
        {
            SetVisible(false);
            return;
        }

        transform.position = _target.Position + _offset;

        if (_faceCamera && Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;

        float normalized = _target.MaxHealth <= 0f ? 0f : _target.CurrentHealth / _target.MaxHealth;
        normalized = Mathf.Clamp01(normalized);

        if (_fill != null)
            _fill.fillAmount = normalized;

        bool visible = !_hideWhenFull || normalized < 0.999f;
        SetVisible(visible);
    }

    private void SetVisible(bool visible)
    {
        if (_visualRoot != null)
        {
            _visualRoot.SetActive(visible);
            return;
        }

        if (_canvas != null)
            _canvas.enabled = visible;
    }
}