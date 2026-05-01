using UnityEngine;

public class UnitHealingSystem
{
    public bool IsValidHealTarget(UnitContext context, IHealTarget target)
    {
        if (target == null)
            return false;

        if (!target.IsAlive)
            return false;

        if (target.TeamColor != context.Owner.TeamColor)
            return false;

        if (!target.NeedsHeal)
            return false;

        return true;
    }

    public bool IsInHealRange(UnitContext context, IHealTarget target)
    {
        if (!IsValidHealTarget(context, target))
            return false;

        return Vector3.Distance(context.Transform.position, target.Position) <= context.Data.AttackRange;
    }

    public bool TryHeal(UnitContext context, IHealTarget target)
    {
        if (context.Owner.UnitType != UnitType.Monk)
            return false;

        if (!IsInHealRange(context, target))
            return false;

        if (context.AttackCooldown > 0f)
            return false;

        if (context.CurrentMana < context.Data.HealManaCost)
            return false;

        context.CurrentMana -= context.Data.HealManaCost;
        target.Heal(context.Data.AttackDamage);
        context.AttackCooldown = context.Data.AttackSpeed > 0f ? 1f / context.Data.AttackSpeed : 0f;

        SpawnHealEffect(context, target);
        return true;
    }

    private void SpawnHealEffect(UnitContext context, IHealTarget target)
    {
        var effectPrefab = context.Owner.HealEffectPrefab;

        if (effectPrefab == null)
            return;

        var effect = Object.Instantiate(effectPrefab, target.Position, Quaternion.identity);
        var follow = effect.GetComponent<FollowWorldTarget>();

        if (follow != null)
            follow.SetTarget(target);
    }
}