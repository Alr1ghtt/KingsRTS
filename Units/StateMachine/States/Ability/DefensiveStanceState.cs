public class DefensiveStanceState : IUnitState
{
    private float _bonusArmor = 5f;

    public void Enter(UnitContext context)
    {
        context.CurrentHealth += _bonusArmor;
    }

    public void Update(UnitContext context, float deltaTime) { }

    public void Exit(UnitContext context)
    {
        context.CurrentHealth -= _bonusArmor;
    }
}