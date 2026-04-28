public readonly struct UnitAttackProfile
{
    public float Duration { get; }
    public float HitTime { get; }
    public bool UsesProjectile { get; }

    private UnitAttackProfile(float duration, float hitTime, bool usesProjectile)
    {
        Duration = duration;
        HitTime = hitTime;
        UsesProjectile = usesProjectile;
    }

    public static UnitAttackProfile Default(float duration, float hitTime)
    {
        return new UnitAttackProfile(duration, hitTime, false);
    }

    public static UnitAttackProfile Projectile(float duration, float hitTime)
    {
        return new UnitAttackProfile(duration, hitTime, true);
    }
}