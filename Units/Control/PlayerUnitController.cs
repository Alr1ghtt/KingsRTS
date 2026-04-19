using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUnitController : MonoBehaviour
{
    private enum InputMode
    {
        Default,
        Move,
        Patrol,
        Attack,
        Repair,
        Build
    }

    [SerializeField] private int _localPlayerId;
    [SerializeField] private Camera _camera;
    [SerializeField] private GroundClickRaycaster _groundClickRaycaster;
    [SerializeField] private SelectionBoxView _selectionBoxView;
    [SerializeField] private LayerMask _unitMask = -1;
    [SerializeField] private float _clickSelectionThreshold = 8f;

    private readonly List<Unit> _selectedUnits = new();

    private InputMode _inputMode;
    private bool _isDraggingSelection;
    private Vector2 _selectionStartScreen;

    private Mouse _mouse;
    private Keyboard _keyboard;

    public int LocalPlayerId => _localPlayerId;

    private void Awake()
    {
        _mouse = Mouse.current;
        _keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (_mouse == null)
            _mouse = Mouse.current;

        if (_keyboard == null)
            _keyboard = Keyboard.current;

        HandleModeHotkeys();
        HandleSelection();
        HandleOrders();
    }

    private void HandleModeHotkeys()
    {
        if (_keyboard == null)
            return;

        if (_keyboard.mKey.wasPressedThisFrame)
            _inputMode = InputMode.Move;

        if (_keyboard.pKey.wasPressedThisFrame)
            _inputMode = InputMode.Patrol;

        if (_keyboard.aKey.wasPressedThisFrame)
            _inputMode = InputMode.Attack;

        if (_keyboard.rKey.wasPressedThisFrame)
            _inputMode = InputMode.Repair;

        if (_keyboard.bKey.wasPressedThisFrame)
            _inputMode = InputMode.Build;

        if (_keyboard.hKey.wasPressedThisFrame)
        {
            if (_selectedUnits.Count > 0)
            {
                var orders = UnitOrderFactory.CreateHoldOrders(_selectedUnits);
                ApplyOrders(orders);
            }

            _inputMode = InputMode.Default;
        }
    }

    private void HandleSelection()
    {
        if (_inputMode != InputMode.Default)
            return;

        if (_mouse == null)
            return;

        if (_mouse.leftButton.wasPressedThisFrame)
        {
            _selectionStartScreen = _mouse.position.ReadValue();
            _isDraggingSelection = true;

            if (_selectionBoxView != null)
            {
                _selectionBoxView.Show();
                _selectionBoxView.UpdateBox(_selectionStartScreen, _selectionStartScreen);
            }
        }

        if (_mouse.leftButton.isPressed && _isDraggingSelection)
        {
            if (_selectionBoxView != null)
                _selectionBoxView.UpdateBox(_selectionStartScreen, _mouse.position.ReadValue());
        }

        if (_mouse.leftButton.wasReleasedThisFrame && _isDraggingSelection)
        {
            _isDraggingSelection = false;

            if (_selectionBoxView != null)
                _selectionBoxView.Hide();

            var currentMousePosition = _mouse.position.ReadValue();
            var dragDistance = Vector2.Distance(_selectionStartScreen, currentMousePosition);
            var additiveSelection = IsShiftPressed();

            if (dragDistance < _clickSelectionThreshold)
                HandleClickSelection(additiveSelection);
            else
                HandleBoxSelection(_selectionStartScreen, currentMousePosition, additiveSelection);
        }
    }

    private void HandleOrders()
    {
        if (_selectedUnits.Count == 0)
            return;

        if (_mouse == null)
            return;

        if (_mouse.rightButton.wasPressedThisFrame)
        {
            HandleContextRightClick();
            return;
        }

        if (!_mouse.leftButton.wasPressedThisFrame)
            return;

        switch (_inputMode)
        {
            case InputMode.Move:
                IssueMoveOrder();
                break;

            case InputMode.Patrol:
                IssuePatrolOrder();
                break;

            case InputMode.Attack:
                IssueAttackOrder();
                break;

            case InputMode.Repair:
                IssueRepairOrder();
                break;

            case InputMode.Build:
                IssueBuildOrder();
                break;
        }
    }

    private void HandleContextRightClick()
    {
        var attackTarget = GetAttackTargetUnderCursor();
        if (attackTarget != null)
        {
            var attackOrders = UnitOrderFactory.CreateAttackTargetOrders(_selectedUnits, attackTarget);
            ApplyOrders(attackOrders);
            _inputMode = InputMode.Default;
            return;
        }

        if (AreAllSelectedWorkers())
        {
            var repairTarget = GetRepairTargetUnderCursor();
            if (repairTarget != null)
            {
                var repairOrders = UnitOrderFactory.CreateRepairOrders(_selectedUnits, repairTarget);
                ApplyOrders(repairOrders);
                _inputMode = InputMode.Default;
                return;
            }
        }

        if (_groundClickRaycaster != null && _groundClickRaycaster.TryGetGroundPoint(out var movePoint))
        {
            var moveOrders = UnitOrderFactory.CreateMoveOrders(_selectedUnits, movePoint);
            ApplyOrders(moveOrders);
            _inputMode = InputMode.Default;
        }
    }

    private void IssueMoveOrder()
    {
        if (_groundClickRaycaster == null)
            return;

        if (!_groundClickRaycaster.TryGetGroundPoint(out var point))
            return;

        var orders = UnitOrderFactory.CreateMoveOrders(_selectedUnits, point);
        ApplyOrders(orders);
        _inputMode = InputMode.Default;
    }

    private void IssuePatrolOrder()
    {
        if (_groundClickRaycaster == null)
            return;

        if (!_groundClickRaycaster.TryGetGroundPoint(out var point))
            return;

        var orders = UnitOrderFactory.CreatePatrolOrders(_selectedUnits, point);
        ApplyOrders(orders);
        _inputMode = InputMode.Default;
    }

    private void IssueAttackOrder()
    {
        var attackTarget = GetAttackTargetUnderCursor();
        if (attackTarget != null)
        {
            var directOrders = UnitOrderFactory.CreateAttackTargetOrders(_selectedUnits, attackTarget);
            ApplyOrders(directOrders);
            _inputMode = InputMode.Default;
            return;
        }

        if (_groundClickRaycaster == null)
            return;

        if (!_groundClickRaycaster.TryGetGroundPoint(out var point))
            return;

        var attackMoveOrders = UnitOrderFactory.CreateAttackMoveOrders(_selectedUnits, point);
        ApplyOrders(attackMoveOrders);
        _inputMode = InputMode.Default;
    }

    private void IssueRepairOrder()
    {
        if (!AreAllSelectedWorkers())
        {
            _inputMode = InputMode.Default;
            return;
        }

        var repairTarget = GetRepairTargetUnderCursor();
        if (repairTarget == null)
        {
            _inputMode = InputMode.Default;
            return;
        }

        var orders = UnitOrderFactory.CreateRepairOrders(_selectedUnits, repairTarget);
        ApplyOrders(orders);
        _inputMode = InputMode.Default;
    }

    private void IssueBuildOrder()
    {
        if (!AreAllSelectedWorkers())
        {
            _inputMode = InputMode.Default;
            return;
        }

        if (_groundClickRaycaster == null)
            return;

        if (!_groundClickRaycaster.TryGetGroundPoint(out var point))
            return;

        var orders = UnitOrderFactory.CreateBuildOrders(_selectedUnits, point);
        ApplyOrders(orders);
        _inputMode = InputMode.Default;
    }

    private void HandleClickSelection(bool additiveSelection)
    {
        var clickedUnit = GetOwnedUnitUnderCursor();

        if (clickedUnit == null)
        {
            if (!additiveSelection)
                ClearSelection();

            return;
        }

        if (!additiveSelection)
            ClearSelection();

        AddToSelection(clickedUnit);
    }

    private void HandleBoxSelection(Vector2 start, Vector2 end, bool additiveSelection)
    {
        if (!additiveSelection)
            ClearSelection();

        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return;

        var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        var min = Vector2.Min(start, end);
        var max = Vector2.Max(start, end);

        for (int i = 0; i < allUnits.Length; i++)
        {
            var unit = allUnits[i];

            if (unit.PlayerId != _localPlayerId)
                continue;

            if (!unit.SelectableByLocalPlayer)
                continue;

            var screenPoint = _camera.WorldToScreenPoint(unit.transform.position);

            if (screenPoint.z < 0f)
                continue;

            if (screenPoint.x < min.x || screenPoint.x > max.x || screenPoint.y < min.y || screenPoint.y > max.y)
                continue;

            AddToSelection(unit);
        }
    }

    private Unit GetOwnedUnitUnderCursor()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null || _mouse == null)
            return null;

        var worldPoint = GetMouseWorldPoint();
        var hit = Physics2D.OverlapPoint(worldPoint, _unitMask);
        if (hit == null)
            return null;

        var unit = hit.GetComponentInParent<Unit>();
        if (unit == null)
            return null;

        if (unit.PlayerId != _localPlayerId)
            return null;

        if (!unit.SelectableByLocalPlayer)
            return null;

        return unit;
    }

    private IAttackTarget GetAttackTargetUnderCursor()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null || _mouse == null)
            return null;

        var worldPoint = GetMouseWorldPoint();
        var hit = Physics2D.OverlapPoint(worldPoint, _unitMask);
        if (hit == null)
            return null;

        var attackTarget = hit.GetComponentInParent<MonoBehaviour>() as IAttackTarget;
        if (attackTarget == null)
            return null;

        if (!attackTarget.IsAlive)
            return null;

        if (attackTarget.PlayerId == _localPlayerId)
            return null;

        return attackTarget;
    }

    private IRepairTarget GetRepairTargetUnderCursor()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null || _mouse == null)
            return null;

        var worldPoint = GetMouseWorldPoint();
        var hit = Physics2D.OverlapPoint(worldPoint, _unitMask);
        if (hit == null)
            return null;

        var repairTarget = hit.GetComponentInParent<MonoBehaviour>() as IRepairTarget;
        if (repairTarget == null)
            return null;

        if (!repairTarget.IsAlive)
            return null;

        if (!repairTarget.CanBeRepaired)
            return null;

        if (!repairTarget.NeedsRepair)
            return null;

        if (repairTarget.PlayerId != _localPlayerId)
            return null;

        return repairTarget;
    }

    private Vector3 GetMouseWorldPoint()
    {
        var mousePosition = _mouse.position.ReadValue();
        var worldPoint = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -_camera.transform.position.z));
        worldPoint.z = 0f;
        return worldPoint;
    }

    private bool AreAllSelectedWorkers()
    {
        if (_selectedUnits.Count == 0)
            return false;

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (!_selectedUnits[i].Data.CanRepair && !_selectedUnits[i].Data.CanBuild)
                return false;
        }

        return true;
    }

    private bool IsShiftPressed()
    {
        if (_keyboard == null)
            return false;

        return _keyboard.leftShiftKey.isPressed || _keyboard.rightShiftKey.isPressed;
    }

    private void ApplyOrders(IReadOnlyList<UnitCommand> orders)
    {
        for (int i = 0; i < _selectedUnits.Count && i < orders.Count; i++)
            _selectedUnits[i].ApplyCommand(orders[i]);
    }

    private void AddToSelection(Unit unit)
    {
        if (_selectedUnits.Contains(unit))
            return;

        _selectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    private void ClearSelection()
    {
        for (int i = 0; i < _selectedUnits.Count; i++)
            _selectedUnits[i].SetSelected(false);

        _selectedUnits.Clear();
    }
}