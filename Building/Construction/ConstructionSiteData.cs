using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionSiteData", menuName = "RTS/Buildings/Construction Site Data")]
public class ConstructionSiteData : ScriptableObject
{
    [SerializeField] private ConstructionSite _constructionSitePrefab;

    public ConstructionSite ConstructionSitePrefab => _constructionSitePrefab;
}