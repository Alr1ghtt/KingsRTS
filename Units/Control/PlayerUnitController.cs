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

    [Header("Building")]
    [SerializeField] private BuildingPlacementSystem _buildingPlacementSystem;
    [SerializeField] private BuildingPlacementPreview _buildingPlacementPreview;
    [SerializeField] private bool _enableBuildDebugLogs = true;
    [SerializeField] private BuildingData _archeryData;
    [SerializeField] private BuildingData _barracksData;
    [SerializeField] private BuildingData _castleData;
    [SerializeField] private BuildingData _houseData;
    [SerializeField] private BuildingData _monasteryData;
    [SerializeField] private BuildingData _towerData;

    private readonly List<Unit> _selectedUnits = new();
    private readonly List<WorkerConstructionAgent> _selectedWorkers = new();

    private InputMode _inputMode;
    private bool _isDraggingSelection;
    private Vector2 _selectionStartScreen;
    private BuildingData _pendingBuildingData;

    private Mouse _mouse;
    private Keyboard _keyboard;

    public int LocalPlayerId => _localPlayerId;
    public List<Unit> SelectedUnits => _selectedUnits;

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

        RefreshSelectedWorkers();
        HandleBuildSelectionHotkeys();
        HandleModeHotkeys();
        HandleSelection();
        HandleOrders();
        UpdateBuildPreview();
    }

    private void CancelSelectedWorkersConstructionOrders()
    {
        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] == null)
                continue;

            _selectedWorkers[i].CancelConstructionOrder();
        }
    }

    private void HandleBuildSelectionHotkeys()
    {
        if (_keyboard == null)
            return;

        if (_inputMode != InputMode.Build)
            return;

        if (_pendingBuildingData == null)
        {
            if (_keyboard.aKey.wasPressedThisFrame)
                SelectPendingBuilding(_archeryData);

            if (_keyboard.bKey.wasPressedThisFrame)
                SelectPendingBuilding(_barracksData);

            if (_keyboard.cKey.wasPressedThisFrame)
                SelectPendingBuilding(_castleData);

            if (_keyboard.hKey.wasPressedThisFrame)
                SelectPendingBuilding(_houseData);

            if (_keyboard.mKey.wasPressedThisFrame)
                SelectPendingBuilding(_monasteryData);

            if (_keyboard.tKey.wasPressedThisFrame)
                SelectPendingBuilding(_towerData);
        }

        if (_keyboard.escapeKey.wasPressedThisFrame)
            CancelBuildMode();
    }

    private void HandleModeHotkeys()
    {
        if (_keyboard == null)
            return;

        if (_inputMode == InputMode.Build && _pendingBuildingData != null)
            return;

        if (_keyboard.mKey.wasPressedThisFrame)
            _inputMode = InputMode.Move;

        if (_keyboard.pKey.wasPressedThisFrame)
            _inputMode = InputMode.Patrol;

        if (_keyboard.aKey.wasPressedThisFrame && _inputMode != InputMode.Build)
            _inputMode = InputMode.Attack;

        if (_keyboard.rKey.wasPressedThisFrame)
            _inputMode = InputMode.Repair;

        if (_keyboard.bKey.wasPressedThisFrame)
        {
            if (AreAllSelectedWorkers())
            {
                _inputMode = InputMode.Build;
                _pendingBuildingData = null;
                LogBuild("Рабочий готов к строительству, идет выбор постройки");
            }
        }

        if (_keyboard.hKey.wasPressedThisFrame && _inputMode != InputMode.Build)
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

        if (_inputMode == InputMode.Build)
        {
            HandleBuildPlacement();
            return;
        }

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
        }
    }
    private Vector3 CalculateBuildApproachPoint(Vector3 buildPoint, Vector3 workerPosition)
    {
        Vector2 buildAreaSize = new Vector2(2.664385f, 2.664385f);
        Vector2 buildAreaOffset = new Vector2(0.008683f, 0f);
        float padding = 0.65f;
        float insideOffset = 0.35f;

        Vector3 center = buildPoint + new Vector3(buildAreaOffset.x, buildAreaOffset.y, 0f);
        Vector3 offset = workerPosition - center;
        offset.z = 0f;

        float halfWidth = buildAreaSize.x * 0.5f + padding;
        float halfHeight = buildAreaSize.y * 0.5f + padding;

        Vector3 approachPoint = center;

        float normalizedX = Mathf.Abs(offset.x) / halfWidth;
        float normalizedY = Mathf.Abs(offset.y) / halfHeight;

        if (normalizedX > normalizedY)
        {
            approachPoint.x += offset.x >= 0f ? halfWidth - insideOffset : -halfWidth + insideOffset;
            approachPoint.y += Mathf.Clamp(offset.y, -halfHeight + insideOffset, halfHeight - insideOffset);
        }
        else
        {
            approachPoint.x += Mathf.Clamp(offset.x, -halfWidth + insideOffset, halfWidth - insideOffset);
            approachPoint.y += offset.y >= 0f ? halfHeight - insideOffset : -halfHeight + insideOffset;
        }

        approachPoint.z = buildPoint.z;
        return approachPoint;
    }
    private void HandleBuildPlacement()
    {
        if (_mouse == null)
            return;

        if (_mouse.rightButton.wasPressedThisFrame)
        {
            CancelBuildMode();
            return;
        }

        if (_pendingBuildingData == null)
            return;

        if (!_mouse.leftButton.wasPressedThisFrame)
            return;

        if (_groundClickRaycaster == null)
            return;

        if (!_groundClickRaycaster.TryGetGroundPoint(out var point))
            return;

        WorkerConstructionAgent firstWorker = GetFirstSelectedWorker();
        if (firstWorker == null)
        {
            LogBuild("Нет выбранного рабочего для строительства");
            CancelBuildMode();
            return;
        }

        Vector3 approachPoint = CalculateBuildApproachPoint(point, firstWorker.transform.position);

        LogBuild($"Выбрана точка строительства {_pendingBuildingData.DisplayName}: {point}, точка подхода: {approachPoint}");

        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] == null)
                continue;

            Vector3 workerApproachPoint = CalculateBuildApproachPoint(point, _selectedWorkers[i].transform.position);
            _selectedWorkers[i].StartPendingBuild(_pendingBuildingData, point, workerApproachPoint, _localPlayerId, _selectedWorkers[i].Unit.TeamColor, _buildingPlacementSystem);
        }

        var moveOrders = UnitOrderFactory.CreateMoveOrders(_selectedUnits, approachPoint);
        ApplyOrders(moveOrders);

        CancelBuildMode();
    }

    private void UpdateBuildPreview()
    {
        if (_buildingPlacementPreview == null)
            return;

        if (_inputMode != InputMode.Build || _pendingBuildingData == null)
        {
            _buildingPlacementPreview.Hide();
            return;
        }

        if (_groundClickRaycaster == null)
        {
            _buildingPlacementPreview.Hide();
            return;
        }

        if (!_groundClickRaycaster.TryGetGroundPoint(out var point))
        {
            _buildingPlacementPreview.Hide();
            return;
        }

        _buildingPlacementPreview.Show(_pendingBuildingData, point);
    }

    private void SelectPendingBuilding(BuildingData buildingData)
    {
        if (buildingData == null)
        {
            LogBuild("BuildingData не назначен");
            return;
        }

        _pendingBuildingData = buildingData;
        LogBuild($"Выбрана постройка: {buildingData.DisplayName}");
    }

    private void CancelBuildMode()
    {
        _pendingBuildingData = null;
        _inputMode = InputMode.Default;

        if (_buildingPlacementPreview != null)
            _buildingPlacementPreview.Hide();

        LogBuild("Режим строительства отменен");
    }

    private void RefreshSelectedWorkers()
    {
        _selectedWorkers.Clear();

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] == null)
                continue;

            WorkerConstructionAgent worker = _selectedUnits[i].GetComponent<WorkerConstructionAgent>();
            if (worker == null)
                continue;

            if (!worker.Unit.Data.CanBuild)
                continue;

            _selectedWorkers.Add(worker);
        }
    }

    private WorkerConstructionAgent GetFirstSelectedWorker()
    {
        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] != null)
                return _selectedWorkers[i];
        }

        return null;
    }

    private void HandleContextRightClick()
    {
        if (TryAssignWorkersToConstructionSiteUnderCursor())
        {
            _inputMode = InputMode.Default;
            return;
        }

        CancelSelectedWorkersConstructionOrders();

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

    private bool TryAssignWorkersToConstructionSiteUnderCursor()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null || _mouse == null)
            return false;

        if (_selectedWorkers.Count == 0)
            return false;

        var worldPoint = GetMouseWorldPoint();
        var hit = Physics2D.OverlapPoint(worldPoint);
        if (hit == null)
            return false;

        ConstructionSite site = hit.GetComponentInParent<ConstructionSite>();
        if (site == null)
            return false;

        WorkerConstructionAgent firstWorker = GetFirstSelectedWorker();
        if (firstWorker == null)
            return false;

        LogBuild($"Назначение рабочих на существующую стройку: {site.BuildingData.DisplayName}");

        for (int i = 0; i < _selectedWorkers.Count; i++)
        {
            if (_selectedWorkers[i] == null)
                continue;

            _selectedWorkers[i].AssignToSite(site);
        }

        Vector3 targetPoint = GetConstructionSiteApproachPoint(site, firstWorker.transform.position);
        var moveOrders = UnitOrderFactory.CreateMoveOrders(_selectedUnits, targetPoint);
        ApplyOrders(moveOrders);

        return true;
    }
    private Vector3 GetConstructionSiteApproachPoint(ConstructionSite site, Vector3 workerPosition)
    {
        if (site == null)
            return workerPosition;

        Vector3 targetPoint = site.GetClosestBuildPerimeterPoint(workerPosition);

        Vector3 toCenter = site.transform.position - targetPoint;
        toCenter.z = 0f;

        if (toCenter.sqrMagnitude > 0.0001f)
        {
            targetPoint += toCenter.normalized * 0.35f;
            targetPoint.z = site.transform.position.z;
        }

        return targetPoint;
    }
    private void IssueMoveOrder()
    {
        CancelSelectedWorkersConstructionOrders();

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
        CancelSelectedWorkersConstructionOrders();

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
        CancelSelectedWorkersConstructionOrders();

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
        CancelSelectedWorkersConstructionOrders();

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

    private void LogBuild(string message)
    {
        if (_enableBuildDebugLogs)
            Debug.Log(message, this);
    }
}