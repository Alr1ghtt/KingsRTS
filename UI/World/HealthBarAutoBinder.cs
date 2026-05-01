using UnityEngine;

public class HealthBarAutoBinder : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _targetBehaviour;
    [SerializeField] private HealthBarWorldView _healthBar;

    private void Awake()
    {
        if (_healthBar == null)
            _healthBar = GetComponent<HealthBarWorldView>();

        if (_targetBehaviour == null)
            _targetBehaviour = GetComponentInParent<MonoBehaviour>();

        var target = _targetBehaviour as IHealthViewTarget;

        if (target != null && _healthBar != null)
            _healthBar.Initialize(target);
    }
}