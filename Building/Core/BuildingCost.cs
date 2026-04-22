using System;

[Serializable]
public struct BuildingCost
{
    public int Gold;
    public int Wood;

    public BuildingCost(int gold, int wood)
    {
        Gold = gold;
        Wood = wood;
    }
}