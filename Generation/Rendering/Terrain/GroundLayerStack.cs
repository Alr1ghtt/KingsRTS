using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundLayerStack : MonoBehaviour
{
    [SerializeField] private Tilemap[] _layers;

    public Tilemap GetLayer(int height)
    {
        if (_layers == null || _layers.Length == 0)
            return null;

        height = Mathf.Clamp(height, 0, _layers.Length - 1);

        return _layers[height];
    }

    public int MaxHeight => _layers.Length;
}