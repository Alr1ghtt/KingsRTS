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
        Build,
        Heal
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

    [Header("HUD")]
    [SerializeField] private RTSHUDView _hudView;
    [SerializeField] private RTSCommandLibrary _commandLibrary;
    [SerializeField] private PlayerResources _playerResources;

    private readonly List<Unit>[] _controlGroups =
    {
    new(),
    new(),
    new(),
    new(),
    new(),
    new()
};

    private bool _isBuildCommandPanelOpen;
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
        BindHUDControlGroupButtons();
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
        HandleControlGroups();
    }
    public void SelectOnly(Unit unit)
    {
        ClearSelection();

        if (unit == null)
            return;

        AddToSelection(unit);
        RefreshHUD();
    }
    private void BindHUDControlGroupButtons()
    {
        if (_hudView == null)
            return;

        var buttons = _hudView.ControlGroupButtons;

        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length && i < _controlGroups.Length; i++)
        {
            var index = i;

            if (buttons[i] == null)
                continue;

            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => SelectControlGroup(index));
        }
    }
    private void HandleControlGroups()
    {
        if (_keyboard == null)
            return;

        if (TryGetControlGroupKey(out var index))
        {
            if (IsCtrlPressed())
                SaveControlGroup(index);
            else
                SelectControlGroup(index);
        }
    }
    private bool TryGetControlGroupKey(out int index)
    {
        index = -1;

        if (_keyboard.digit1Key.wasPressedThisFrame)
            index = 0;
        else if (_keyboard.digit2Key.wasPressedThisFrame)
            index = 1;
        else if (_keyboard.digit3Key.wasPressedThisFrame)
            index = 2;
        else if (_keyboard.digit4Key.wasPressedThisFrame)
            index = 3;
        else if (_keyboard.digit5Key.wasPressedThisFrame)
            index = 4;
        else if (_keyboard.digit6Key.wasPressedThisFrame)
            index = 5;

        return index >= 0;
    }
    private bool IsCtrlPressed()
    {
        if (_keyboard == null)
            return false;

        return _keyboard.leftCtrlKey.isPressed || _keyboard.rightCtrlKey.isPressed;
    }
    private void SaveControlGroup(int index)
    {
        if (index < 0 || index >= _controlGroups.Length)
            return;

        _controlGroups[index].Clear();

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] == null)
                continue;

            _controlGroups[index].Add(_selectedUnits[i]);
        }

        RefreshHUD();
    }
    private void SelectControlGroup(int index)
    {
        if (index < 0 || index >= _controlGroups.Length)
            return;

        ClearSelection();

        for (int i = _controlGroups[index].Count - 1; i >= 0; i--)
        {
            var unit = _controlGroups[index][i];

            if (unit == null || !unit.IsAlive)
            {
                _controlGroups[index].RemoveAt(i);
                continue;
            }

            AddToSelection(unit);
        }

        _inputMode = InputMode.Default;
        _isBuildCommandPanelOpen = false;
        RefreshHUD();
    }
    private void RefreshHUD()
    {
        if (_hudView == null)
            return;

        if (_playerResources != null)
            _hudView.SetResources(_playerResources);

        for (int i = 0; i < _controlGroups.Length; i++)
            _hudView.SetControlGroupCount(i, GetAliveControlGroupCount(i));

        if (_selectedUnits.Count == 1)
            _hudView.ShowSingleUnit(_selectedUnits[0]);
        else
            _hudView.ShowUnitList(_selectedUnits, SelectOnly);

        _hudView.SetCommands(GetAvailableCommands(), HandleUICommand);
    }
    private int GetAliveControlGroupCount(int index)
    {
        if (index < 0 || index >= _controlGroups.Length)
            return 0;

        var count = 0;

        for (int i = _controlGroups[index].Count - 1; i >= 0; i--)
        {
            var unit = _controlGroups[index][i];

            if (unit == null || !unit.IsAlive)
            {
                _controlGroups[index].RemoveAt(i);
                continue;
            }

            count++;
        }

        return count;
    }
    private IReadOnlyList<RTSCommandDefinition> GetAvailableCommands()
    {
        var commands = new List<RTSCommandDefinition>();

        if (_commandLibrary == null)
            return commands;

        if (_selectedUnits.Count == 0)
            return commands;

        if (_isBuildCommandPanelOpen)
        {
            AddCommand(commands, _commandLibrary.BuildArchery);
            AddCommand(commands, _commandLibrary.BuildBarracks);
            AddCommand(commands, _commandLibrary.BuildCastle);
            AddCommand(commands, _commandLibrary.BuildHouse);
            AddCommand(commands, _commandLibrary.BuildMonastery);
            AddCommand(commands, _commandLibrary.BuildTower);
            return commands;
        }

        AddCommand(commands, _commandLibrary.Move);
        AddCommand(commands, _commandLibrary.HoldPosition);
        AddCommand(commands, _commandLibrary.Patrol);

        if (IsOnlySelectedType(UnitType.Monk))
        {
            AddCommand(commands, _commandLibrary.Heal);
            return commands;
        }

        if (HasAnySelectedAttacker())
            AddCommand(commands, _commandLibrary.Attack);

        if (AreAllSelectedWorkers())
        {
            AddCommand(commands, _commandLibrary.Repair);
            AddCommand(commands, _commandLibrary.BuildMenu);
        }

        return commands;
    }

    private void AddCommand(List<RTSCommandDefinition> commands, RTSCommandDefinition command)
    {
        if (command == null)
            return;

        commands.Add(command);
    }

    private bool IsOnlySelectedType(UnitType type)
    {
        if (_selectedUnits.Count == 0)
            return false;

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] == null)
                return false;

            if (_selectedUnits[i].UnitType != type)
                return false;
        }

        return true;
    }
    private bool HasSelectedMonk()
    {
        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] == null)
                continue;

            if (_selectedUnits[i].UnitType == UnitType.Monk)
                return true;
        }

        return false;
    }
    private bool HasAnySelectedAttacker()
    {
        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] == null)
                continue;

            if (_selectedUnits[i].CanAttack)
                return true;
        }

        return false;
    }
    private void HandleUICommand(RTSCommandDefinition command)
    {
        if (command == null)
            return;

        switch (command.Type)
        {
            case RTSCommandType.Move:
                _inputMode = InputMode.Move;
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.HoldPosition:
                if (_selectedUnits.Count > 0)
                {
                    var orders = UnitOrderFactory.CreateHoldOrders(_selectedUnits);
                    ApplyOrders(orders);
                }

                _inputMode = InputMode.Default;
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.Patrol:
                _inputMode = InputMode.Patrol;
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.Attack:
                _inputMode = InputMode.Attack;
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.Heal:
                _inputMode = InputMode.Heal;
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.Repair:
                _inputMode = InputMode.Repair;
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.BuildMenu:
                if (AreAllSelectedWorkers())
                {
                    _inputMode = InputMode.Build;
                    _pendingBuildingData = null;
                    _isBuildCommandPanelOpen = true;
                }
                break;

            case RTSCommandType.BuildArchery:
                SelectPendingBuilding(_archeryData);
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.BuildBarracks:
                SelectPendingBuilding(_barracksData);
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.BuildCastle:
                SelectPendingBuilding(_castleData);
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.BuildHouse:
                SelectPendingBuilding(_houseData);
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.BuildMonastery:
                SelectPendingBuilding(_monasteryData);
                _isBuildCommandPanelOpen = false;
                break;

            case RTSCommandType.BuildTower:
                SelectPendingBuilding(_towerData);
                _isBuildCommandPanelOpen = false;
                break;
        }

        RefreshHUD();
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
    private void IssueHealOrder()
    {
        var healTarget = GetHealTargetUnderCursor();

        if (healTarget == null)
        {
            _inputMode = InputMode.Default;
            return;
        }

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            var unit = _selectedUnits[i];

            if (unit == null)
                continue;

            if (unit.UnitType != UnitType.Monk)
                continue;

            unit.ApplyCommand(UnitCommand.Heal(healTarget));
        }

        _inputMode = InputMode.Default;
    }
    private IHealTarget GetHealTargetUnderCursor()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null || _mouse == null)
            return null;

        var worldPoint = GetMouseWorldPoint();
        var hit = Physics2D.OverlapPoint(worldPoint, _unitMask);

        if (hit == null)
            return null;

        var healTarget = hit.GetComponentInParent<MonoBehaviour>() as IHealTarget;

        if (healTarget == null)
            return null;

        if (!healTarget.IsAlive)
            return null;

        if (healTarget.TeamColor != GetSelectedTeamColor())
            return null;

        if (!healTarget.NeedsHeal)
            return null;

        return healTarget;
    }
    private TeamColor GetSelectedTeamColor()
    {
        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] != null)
                return _selectedUnits[i].TeamColor;
        }

        return TeamColor.Black;
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
            case InputMode.Heal:
                IssueHealOrder();
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

            RefreshHUD();
            return;
        }

        if (!additiveSelection)
            ClearSelection();

        AddToSelection(clickedUnit);
        RefreshHUD();
    }

    private void HandleBoxSelection(Vector2 start, Vector2 end, bool additiveSelection)
    {
        if (!additiveSelection)
            ClearSelection();

        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
        {
            RefreshHUD();
            return;
        }

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

        RefreshHUD();
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

        if (attackTarget.TeamColor == GetSelectedTeamColor())
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
        if (unit == null)
            return;

        if (_selectedUnits.Contains(unit))
            return;

        _selectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    private void ClearSelection()
    {
        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] != null)
                _selectedUnits[i].SetSelected(false);
        }

        _selectedUnits.Clear();
    }

    private void LogBuild(string message)
    {
        if (_enableBuildDebugLogs)
            Debug.Log(message, this);
    }
}