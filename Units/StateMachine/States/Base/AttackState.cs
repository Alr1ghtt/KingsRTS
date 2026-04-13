using UnityEngine;

public class AttackState : IUnitState
{
    public void Enter(UnitContext context) { }

    public void Update(UnitContext context, float deltaTime)
    {
        if (context.TargetUnit == null)
            return;

        var distance = Vector3.Distance(context.Transform.position, context.TargetUnit.Context.Transform.position);

        if (distance > context.Data.AttackRange)
            return;

        if (context.AttackCooldown > 0f)
            return;

        context.TargetUnit.TakeDamage(context.Data.AttackDamage);
        context.AttackCooldown = 1f / context.Data.AttackSpeed;
    }

    public void Exit(UnitContext context) { }
}