using UnityEngine;

[CreateAssetMenu(fileName = "UnitConfig", menuName = "RTS/Units/Unit Config")]
public class UnitConfig : ScriptableObject
{
    [SerializeField] private UnitData _data;

    public UnitData Data => _data;
}