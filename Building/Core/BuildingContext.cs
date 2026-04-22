using UnityEngine;
using UnityEngine.UIElements;

public class BuildingContext
{
    private readonly Building _building;
    private readonly BuildingData _data;
    private readonly int _ownerPlayerId;
    private readonly TeamColor _teamColor;
    private readonly Transform _transform;

    public Building Building => _building;
    public BuildingData Data => _data;
    public int OwnerPlayerId => _ownerPlayerId;
    public TeamColor TeamColor => _teamColor;
    public Transform Transform => _transform;

    public BuildingContext(Building building, BuildingData data, int ownerPlayerId, TeamColor teamColor, Transform transform)
    {
        _building = building;
        _data = data;
        _ownerPlayerId = ownerPlayerId;
        _teamColor = teamColor;
        _transform = transform;
    }
}