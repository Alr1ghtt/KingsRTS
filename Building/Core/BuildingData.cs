using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "RTS/Buildings/Building Data")]
public class BuildingData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private BuildingType _buildingType;
    [SerializeField] private int _maxHealth = 1000;
    [SerializeField] private int _armor;
    [SerializeField] private BuildingCost _cost;
    [SerializeField] private BuildingUnitType[] _producedUnits;
    [SerializeField] private BuildingUnitType[] _garrisonUnits;
    [SerializeField] private bool _canUpgradeUnits;
    [SerializeField] private float _buildTime = 10f;
    [SerializeField] private Building _buildingPrefab;
    [SerializeField] private Sprite _previewSprite;

    public string Id => _id;
    public string DisplayName => _displayName;
    public BuildingType BuildingType => _buildingType;
    public int MaxHealth => _maxHealth;
    public int Armor => _armor;
    public BuildingCost Cost => _cost;
    public BuildingUnitType[] ProducedUnits => _producedUnits;
    public BuildingUnitType[] GarrisonUnits => _garrisonUnits;
    public bool CanUpgradeUnits => _canUpgradeUnits;
    public float BuildTime => _buildTime;
    public Building BuildingPrefab => _buildingPrefab;
    public Sprite PreviewSprite => _previewSprite;

    public bool CanProduce(BuildingUnitType unitType)
    {
        if (_producedUnits == null)
            return false;

        for (int i = 0; i < _producedUnits.Length; i++)
        {
            if (_producedUnits[i] == unitType)
                return true;
        }

        return false;
    }

    public bool CanGarrison(BuildingUnitType unitType)
    {
        if (_garrisonUnits == null)
            return false;

        for (int i = 0; i < _garrisonUnits.Length; i++)
        {
            if (_garrisonUnits[i] == unitType)
                return true;
        }

        return false;
    }
}