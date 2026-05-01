using UnityEngine;

public interface IHealthViewTarget
{
    bool IsAlive { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
    Vector3 Position { get; }
}