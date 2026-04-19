using System.Collections.Generic;
using UnityEngine;

public static class UnitFormationSystem
{
    public static List<Vector3> CreateFormation(IReadOnlyList<Unit> units, Vector3 center)
    {
        var result = new List<Vector3>(units.Count);
        if (units == null || units.Count == 0)
            return result;

        var slotCount = units.Count;
        var columns = Mathf.CeilToInt(Mathf.Sqrt(slotCount));
        var rows = Mathf.CeilToInt((float)slotCount / columns);
        var spacing = CalculateSpacing(units);

        var slots = new List<Vector3>(slotCount);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (slots.Count >= slotCount)
                    break;

                var offsetX = (column - (columns - 1) * 0.5f) * spacing;
                var offsetY = ((rows - 1) * 0.5f - row) * spacing;
                slots.Add(center + new Vector3(offsetX, offsetY, 0f));
            }
        }

        var remainingSlots = new List<Vector3>(slots);
        var assignedSlots = new Dictionary<Unit, Vector3>(units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            var unitPosition = unit.transform.position;

            var bestSlotIndex = 0;
            var bestDistance = float.MaxValue;

            for (int j = 0; j < remainingSlots.Count; j++)
            {
                var distance = Vector3.SqrMagnitude(unitPosition - remainingSlots[j]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSlotIndex = j;
                }
            }

            assignedSlots[unit] = remainingSlots[bestSlotIndex];
            remainingSlots.RemoveAt(bestSlotIndex);
        }

        for (int i = 0; i < units.Count; i++)
            result.Add(assignedSlots[units[i]]);

        return result;
    }

    private static float CalculateSpacing(IReadOnlyList<Unit> units)
    {
        var maxRadius = 0.35f;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == null)
                continue;

            var radius = units[i].Data.Radius;
            if (radius > maxRadius)
                maxRadius = radius;
        }

        return maxRadius * 2.8f;
    }
}