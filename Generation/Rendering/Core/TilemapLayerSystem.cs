using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapLayerSystem : MonoBehaviour
{
    [SerializeField] private Tilemap _water;

    [SerializeField] private Tilemap _baseCliff;
    public Tilemap BaseCliff => _baseCliff;

    [Header("Ground")]
    [SerializeField] private Tilemap[] _groundLayers;

    [Header("Cliffs")]
    [SerializeField] private Tilemap[] _cliffLayers;

    [Header("Shadows")]
    [SerializeField] private Tilemap[] _shadowLayers;

    [SerializeField] private Tilemap _foam;



    public Tilemap Water => _water;
    public Tilemap Foam => _foam;

    public Tilemap GetGroundLayer(int height)
    {
        int index = height - 1;

        if (index < 0) index = 0;
        if (index >= _groundLayers.Length) index = _groundLayers.Length - 1;

        return _groundLayers[index];
    }

    public Tilemap GetCliffLayer(int height)
    {
        int index = height - 1;

        if (index < 0) index = 0;
        if (index >= _cliffLayers.Length) index = _cliffLayers.Length - 1;

        return _cliffLayers[index];
    }

    public Tilemap GetShadowLayer(int height)
    {
        int index = height - 1;

        if (index < 0) index = 0;
        if (index >= _shadowLayers.Length) index = _shadowLayers.Length - 1;

        return _shadowLayers[index];
    }

    public void ClearAll()
    {
        if (_water) _water.ClearAllTiles();
        if (_foam) _foam.ClearAllTiles();

        foreach (var g in _groundLayers)
            if (g) g.ClearAllTiles();

        foreach (var c in _cliffLayers)
            if (c) c.ClearAllTiles();

        foreach (var s in _shadowLayers)
            if (s) s.ClearAllTiles();
    }
}