using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [SerializeField] private HeightBasedGroundRenderer _groundRenderer;
    [SerializeField] private CliffRenderer _cliffRenderer;

    public void Render(MapData map)
    {
        _groundRenderer.Render(map);
    }
}