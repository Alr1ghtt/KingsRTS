using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapLayerSystem : MonoBehaviour
{
    [SerializeField] private Tilemap[] _groundLayers;

    [SerializeField] private Tilemap _cliffLayer;

    [SerializeField] private Tilemap _decorationLayer;

    public Tilemap GetGroundLayer(int height)
    {
        if (_groundLayers == null || _groundLayers.Length == 0)
            return null;

        height = Mathf.Clamp(height, 0, _groundLayers.Length - 1);

        return _groundLayers[height];
    }

    public Tilemap CliffLayer => _cliffLayer;

    public Tilemap DecorationLayer => _decorationLayer;

    public int GroundLayerCount => _groundLayers.Length;
}