using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    [Header("Edge Scroll")]
    [SerializeField] private bool _useEdgeScroll = true;
    [SerializeField] private float _edgeScrollSpeed = 12f;
    [SerializeField] private float _edgeScrollThreshold = 24f;

    [Header("Middle Mouse Drag")]
    [SerializeField] private bool _useMiddleMouseDrag = true;

    [Header("Bounds")]
    [SerializeField] private bool _useBounds = false;
    [SerializeField] private Vector2 _minBounds;
    [SerializeField] private Vector2 _maxBounds;

    private Mouse _mouse;
    private bool _isMiddleMouseDragging;
    private Vector2 _dragStartMouseScreenPosition;
    private Vector3 _dragStartCameraPosition;

    private void Awake()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();

        if (_camera == null)
            _camera = Camera.main;

        _mouse = Mouse.current;
    }

    private void Update()
    {
        if (_mouse == null)
            _mouse = Mouse.current;

        if (_camera == null || _mouse == null)
            return;

        HandleMiddleMouseDrag();
        HandleEdgeScroll();
        ClampCameraPosition();
    }

    private void HandleMiddleMouseDrag()
    {
        if (!_useMiddleMouseDrag)
            return;

        if (_mouse.middleButton.wasPressedThisFrame)
            BeginMiddleMouseDrag();

        if (_mouse.middleButton.isPressed && _isMiddleMouseDragging)
            UpdateMiddleMouseDrag();

        if (_mouse.middleButton.wasReleasedThisFrame)
            EndMiddleMouseDrag();
    }

    private void BeginMiddleMouseDrag()
    {
        _isMiddleMouseDragging = true;
        _dragStartMouseScreenPosition = _mouse.position.ReadValue();
        _dragStartCameraPosition = transform.position;
    }

    private void UpdateMiddleMouseDrag()
    {
        var currentMouseScreenPosition = _mouse.position.ReadValue();
        var screenDelta = currentMouseScreenPosition - _dragStartMouseScreenPosition;

        var worldUnitsPerPixel = (_camera.orthographicSize * 2f) / Screen.height;
        var worldDelta = new Vector3(screenDelta.x, screenDelta.y, 0f) * worldUnitsPerPixel;

        var targetPosition = _dragStartCameraPosition - worldDelta;
        targetPosition.z = _dragStartCameraPosition.z;

        transform.position = targetPosition;
    }

    private void EndMiddleMouseDrag()
    {
        _isMiddleMouseDragging = false;
    }

    private void HandleEdgeScroll()
    {
        if (!_useEdgeScroll)
            return;

        if (_isMiddleMouseDragging)
            return;

        var mousePosition = _mouse.position.ReadValue();
        var moveDirection = Vector3.zero;

        if (mousePosition.x <= _edgeScrollThreshold)
            moveDirection.x -= 1f;
        else if (mousePosition.x >= Screen.width - _edgeScrollThreshold)
            moveDirection.x += 1f;

        if (mousePosition.y <= _edgeScrollThreshold)
            moveDirection.y -= 1f;
        else if (mousePosition.y >= Screen.height - _edgeScrollThreshold)
            moveDirection.y += 1f;

        if (moveDirection.sqrMagnitude <= 0f)
            return;

        moveDirection.Normalize();

        var delta = moveDirection * _edgeScrollSpeed * Time.unscaledDeltaTime;
        delta.z = 0f;
        transform.position += delta;
    }

    private void ClampCameraPosition()
    {
        if (!_useBounds)
            return;

        var position = transform.position;
        position.x = Mathf.Clamp(position.x, _minBounds.x, _maxBounds.x);
        position.y = Mathf.Clamp(position.y, _minBounds.y, _maxBounds.y);
        transform.position = position;
    }
}