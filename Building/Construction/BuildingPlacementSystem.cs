using UnityEngine;

public class BuildingPlacementSystem : MonoBehaviour
{
    [SerializeField] private BuildingConstructionSystem _constructionSystem;
    [SerializeField] private LayerMask _groundMask = -1;

    public bool TryPlaceBuilding(BuildingData buildingData, Vector3 worldPosition, int ownerPlayerId, TeamColor teamColor, out ConstructionSite site)
    {
        site = null;

        if (buildingData == null)
            return false;

        Vector3 placePosition = new Vector3(worldPosition.x, worldPosition.y, 0f);

        site = _constructionSystem.CreateConstructionSite(buildingData, placePosition, Quaternion.identity, ownerPlayerId, teamColor);
        return site != null;
    }
}