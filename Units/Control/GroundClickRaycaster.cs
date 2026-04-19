using UnityEngine;
using UnityEngine.InputSystem;

public class GroundClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    public bool TryGetGroundPoint(out Vector3 point)
    {
        point = default;

        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return false;

        if (Mouse.current == null)
            return false;

        var mousePosition = Mouse.current.position.ReadValue();
        var worldPoint = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -_camera.transform.position.z));
        worldPoint.z = 0f;
        point = worldPoint;
        return true;
    }
}