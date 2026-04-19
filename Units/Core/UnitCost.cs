using System;
using UnityEngine;

[Serializable]
public class UnitCost
{
    [SerializeField] private int _wool;
    [SerializeField] private int _meat;
    [SerializeField] private int _gold;

    public int Wool => _wool;
    public int Meat => _meat;
    public int Gold => _gold;
}