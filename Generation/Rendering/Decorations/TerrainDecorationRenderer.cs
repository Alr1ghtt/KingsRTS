using UnityEngine;

public class TerrainDecorationRenderer : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabs;
    [SerializeField] private float _spawnChance = 0.05f;

    public GameObject[] Prefabs => _prefabs;
    public float SpawnChance => _spawnChance;

    public void Render(MapData data)
    {
        for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
            {
                MapTile tile = data.GetTile(x, y);

                if (tile.Type == TileType.Terrain && Random.value < _spawnChance)
                {
                    GameObject prefab = _prefabs[Random.Range(0, _prefabs.Length)];
                    Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
                }
            }
    }
}