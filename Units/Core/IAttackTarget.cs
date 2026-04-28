using UnityEngine;

public interface IAttackTarget
{
    int PlayerId { get; }
    TeamColor TeamColor { get; }
    bool IsAlive { get; }
    Vector3 Position { get; }
    void TakeDamage(float damage);
}