public class HealAreaState : IUnitState
{
    public void Enter(UnitContext context) { }

    public void Update(UnitContext context, float deltaTime)
    {
        if (context.AttackCooldown > 0f)
            return;

        context.AttackCooldown = 3f;
    }

    public void Exit(UnitContext context) { }
}