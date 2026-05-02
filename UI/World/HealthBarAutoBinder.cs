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
            _targetBehaviour = FindHealthTargetBehaviour();

        var target = _targetBehaviour as IHealthViewTarget;

        if (target != null && _healthBar != null)
            _healthBar.Initialize(target);
        else
            Debug.LogWarning($"HealthBarAutoBinder не нашел IHealthViewTarget для {name}", this);
    }

    private MonoBehaviour FindHealthTargetBehaviour()
    {
        var behaviours = GetComponentsInParent<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IHealthViewTarget)
                return behaviours[i];
        }

        return null;
    }
}