using UnityEngine;

public class DeathSmoke : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 2f;

    private void Awake()
    {
        Destroy(gameObject, _lifeTime);
    }
}