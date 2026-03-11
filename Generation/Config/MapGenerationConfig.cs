using UnityEngine;

[CreateAssetMenu(menuName = "MapGeneration/Config")]
public class MapGenerationConfig : ScriptableObject
{
    [SerializeField] private int _width = 128;
    [SerializeField] private int _height = 128;
    [SerializeField] private int _seed = 0;


    [Header("Height")]
    [SerializeField] private int _levels = 3;
    [SerializeField] private float _heightNoiseScale = 0.05f;
    
    public int Width => _width;
    public int Height => _height;
    public int Seed => _seed;

    public int Levels => _levels;
    public float HeightNoiseScale => _heightNoiseScale;
}