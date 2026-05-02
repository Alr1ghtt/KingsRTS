using UnityEngine;

public class UnitCommand
{
    private UnitCommandType _type;
    private Vector3 _positionA;
    private Vector3 _positionB;
    private IAttackTarget _attackTarget;
    private IRepairTarget _repairTarget;
    private IHealTarget _healTarget;

    public UnitCommandType Type => _type;
    public Vector3 PositionA => _positionA;
    public Vector3 PositionB => _positionB;
    public IAttackTarget TargetToAttack => _attackTarget;
    public IRepairTarget TargetToRepair => _repairTarget;
    public IHealTarget TargetToHeal => _healTarget;
    public static UnitCommand Stop()
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Stop
        };
    }
    public static UnitCommand Heal(IHealTarget target)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Heal,
            _healTarget = target
        };
    }
    public static UnitCommand Move(Vector3 position)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Move,
            _positionA = position
        };
    }

    public static UnitCommand HoldPosition(Vector3 position)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.HoldPosition,
            _positionA = position
        };
    }

    public static UnitCommand Patrol(Vector3 pointA, Vector3 pointB)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Patrol,
            _positionA = pointA,
            _positionB = pointB
        };
    }

    public static UnitCommand AttackMove(Vector3 position)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.AttackMove,
            _positionA = position
        };
    }

    public static UnitCommand Attack(IAttackTarget target)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.AttackTarget,
            _attackTarget = target
        };
    }

    public static UnitCommand Repair(IRepairTarget target)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Repair,
            _repairTarget = target
        };
    }

    public static UnitCommand Build(Vector3 position)
    {
        return new UnitCommand
        {
            _type = UnitCommandType.Build,
            _positionA = position
        };
    }
}