using System.Collections.Generic;
using UnityEngine;

public static class UnitOrderFactory
{
    public static List<UnitCommand> CreateMoveOrders(IReadOnlyList<Unit> units, Vector3 targetPoint)
    {
        var positions = UnitFormationSystem.CreateFormation(units, targetPoint);
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.Move(positions[i]));

        return orders;
    }

    public static List<UnitCommand> CreateAttackMoveOrders(IReadOnlyList<Unit> units, Vector3 targetPoint)
    {
        var positions = UnitFormationSystem.CreateFormation(units, targetPoint);
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.AttackMove(positions[i]));

        return orders;
    }

    public static List<UnitCommand> CreateHoldOrders(IReadOnlyList<Unit> units)
    {
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.HoldPosition(units[i].transform.position));

        return orders;
    }

    public static List<UnitCommand> CreatePatrolOrders(IReadOnlyList<Unit> units, Vector3 destinationPoint)
    {
        var positions = UnitFormationSystem.CreateFormation(units, destinationPoint);
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.Patrol(units[i].transform.position, positions[i]));

        return orders;
    }

    public static List<UnitCommand> CreateAttackTargetOrders(IReadOnlyList<Unit> units, IAttackTarget target)
    {
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.Attack(target));

        return orders;
    }

    public static List<UnitCommand> CreateRepairOrders(IReadOnlyList<Unit> units, IRepairTarget target)
    {
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.Repair(target));

        return orders;
    }

    public static List<UnitCommand> CreateBuildOrders(IReadOnlyList<Unit> units, Vector3 point)
    {
        var positions = UnitFormationSystem.CreateFormation(units, point);
        var orders = new List<UnitCommand>(units.Count);

        for (int i = 0; i < units.Count; i++)
            orders.Add(UnitCommand.Build(positions[i]));

        return orders;
    }
}