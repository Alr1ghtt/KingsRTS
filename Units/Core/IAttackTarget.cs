using UnityEngine;

public interface IAttackTarget
{
    int PlayerId => 0;
    bool IsAlive => false;
    Vector3 Position => Vector3.zero;
    void TakeDamage(float damage);
}