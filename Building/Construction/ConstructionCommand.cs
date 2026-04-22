public class ConstructionCommand
{
    private readonly BuildingData _buildingData;

    public BuildingData BuildingData => _buildingData;

    public ConstructionCommand(BuildingData buildingData)
    {
        _buildingData = buildingData;
    }
}