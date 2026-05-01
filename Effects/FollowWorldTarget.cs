using UnityEngine;

public class FollowWorldTarget : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 0.8f;
    [SerializeField] private Vector3 _offset;

    private IHealTarget _target;
    private float _timer;

    public void SetTarget(IHealTarget target)
    {
        _target = target;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_target != null && _target.IsAlive)
            transform.position = _target.Position + _offset;

        if (_timer >= _lifeTime)
            Destroy(gameObject);
    }
}