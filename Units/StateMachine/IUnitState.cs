public interface IUnitState
{
    void Enter(UnitContext context);
    void Update(UnitContext context, float deltaTime);
    void Exit(UnitContext context);
}