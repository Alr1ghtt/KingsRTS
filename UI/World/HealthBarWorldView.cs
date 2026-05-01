using UnityEngine;
using UnityEngine.UI;

public class HealthBarWorldView : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private GameObject _root;
    [SerializeField] private bool _hideWhenFull = true;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0.75f, 0f);

    private IHealthViewTarget _target;

    public void Initialize(IHealthViewTarget target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null || !_target.IsAlive)
        {
            if (_root != null)
                _root.SetActive(false);

            return;
        }

        transform.position = _target.Position + _offset;

        var normalized = _target.MaxHealth <= 0f ? 0f : _target.CurrentHealth / _target.MaxHealth;

        if (_fill != null)
            _fill.fillAmount = Mathf.Clamp01(normalized);

        if (_root != null)
            _root.SetActive(!_hideWhenFull || normalized < 0.999f);
    }
}