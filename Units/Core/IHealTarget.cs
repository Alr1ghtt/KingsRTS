using UnityEngine;

public interface IHealTarget
{
    TeamColor TeamColor { get; }
    bool IsAlive { get; }
    bool NeedsHeal { get; }
    Vector3 Position { get; }
    void Heal(float amount);
}