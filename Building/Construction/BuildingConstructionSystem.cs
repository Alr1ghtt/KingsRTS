using System.Collections.Generic;
using UnityEngine;

public class BuildingConstructionSystem : MonoBehaviour
{
    [SerializeField] private ConstructionSiteData _constructionSiteData;

    private readonly List<ConstructionSite> _sites = new List<ConstructionSite>();

    public ConstructionSite CreateConstructionSite(BuildingData buildingData, Vector3 position, Quaternion rotation, int ownerPlayerId, TeamColor teamColor)
    {
        if (_constructionSiteData == null || _constructionSiteData.ConstructionSitePrefab == null)
            return null;

        ConstructionSite site = Instantiate(_constructionSiteData.ConstructionSitePrefab, position, rotation);
        site.Initialize(buildingData, ownerPlayerId, teamColor);
        _sites.Add(site);
        return site;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = _sites.Count - 1; i >= 0; i--)
        {
            if (_sites[i] == null)
            {
                _sites.RemoveAt(i);
                continue;
            }

            _sites[i].Tick(deltaTime);

            if (_sites[i] == null)
                _sites.RemoveAt(i);
        }
    }
}