using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RTSHUDView : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private TMP_Text _foodText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _woodText;
    [SerializeField] private TMP_Text _combatLimitText;
    [SerializeField] private TMP_Text _workerCountText;

    [Header("Control Groups")]
    [SerializeField] private Button[] _controlGroupButtons;
    [SerializeField] private TMP_Text[] _controlGroupCountTexts;

    [Header("Selection")]
    [SerializeField] private Transform _selectionIconRoot;
    [SerializeField] private SelectionIconView _selectionIconPrefab;
    [SerializeField] private GameObject _singleSelectionPanel;
    [SerializeField] private Image _singleSelectionIcon;
    [SerializeField] private TMP_Text _singleSelectionNameText;
    [SerializeField] private TMP_Text _singleSelectionStatsText;

    [Header("Commands")]
    [SerializeField] private Transform _commandButtonRoot;
    [SerializeField] private CommandButtonView _commandButtonPrefab;

    private readonly List<SelectionIconView> _selectionIcons = new();
    private readonly List<CommandButtonView> _commandButtons = new();

    private string _lastCommandSignature;
    private int _lastSelectionCount = -1;
    private Unit _lastSingleUnit;

    private RTSCommandType? _activeCommandType;
    public Button[] ControlGroupButtons => _controlGroupButtons;

    private void Start()
    {
        if (_singleSelectionPanel != null)
            _singleSelectionPanel.SetActive(false);

        ClearSelectionIcons();
        ClearCommandButtons();
    }

    public void SetResources(PlayerResources resources)
    {
        if (resources == null)
            return;

        if (_foodText != null)
            _foodText.text = resources.Food.ToString();

        if (_goldText != null)
            _goldText.text = resources.Gold.ToString();

        if (_woodText != null)
            _woodText.text = resources.Wood.ToString();

        if (_combatLimitText != null)
            _combatLimitText.text = $"{resources.CombatUsed}/{resources.CombatLimit}";

        if (_workerCountText != null)
            _workerCountText.text = resources.WorkerCount.ToString();
    }

    public void SetControlGroupCount(int index, int count)
    {
        if (_controlGroupCountTexts == null)
            return;

        if (index < 0 || index >= _controlGroupCountTexts.Length)
            return;

        if (_controlGroupCountTexts[index] == null)
            return;

        _controlGroupCountTexts[index].text = count > 0 ? count.ToString() : string.Empty;
    }
    public void RefreshDynamicSelectionStats(IReadOnlyList<Unit> units)
    {
        if (units == null)
            return;

        if (units.Count != 1)
            return;

        UpdateSingleUnitStats(units[0]);
    }
    public void ShowSingleUnit(Unit unit)
    {
        if (_lastSelectionCount == 1 && _lastSingleUnit == unit)
        {
            UpdateSingleUnitStats(unit);
            return;
        }

        _lastSelectionCount = 1;
        _lastSingleUnit = unit;

        ClearSelectionIcons();

        if (_singleSelectionPanel != null)
            _singleSelectionPanel.SetActive(unit != null);

        UpdateSingleUnitStats(unit);
    }
    private void UpdateSingleUnitStats(Unit unit)
    {
        if (unit == null)
            return;

        if (_singleSelectionIcon != null)
            _singleSelectionIcon.sprite = unit.Icon;

        if (_singleSelectionNameText != null)
            _singleSelectionNameText.text = unit.UnitType.ToString();

        if (_singleSelectionStatsText == null)
            return;

        var text = $"HP: {Mathf.CeilToInt(unit.Context.CurrentHealth)}/{Mathf.CeilToInt(unit.Data.MaxHealth)}";

        if (unit.UnitType == UnitType.Monk)
            text += $"\nMana: {Mathf.CeilToInt(unit.Context.CurrentMana)}/{Mathf.CeilToInt(unit.Data.MaxMana)}";

        if (unit.CanAttack)
            text += $"\nDamage: {unit.Data.AttackDamage}";

        if (unit.UnitType == UnitType.Monk)
            text += $"\nHeal: {unit.Data.AttackDamage}";

        text += $"\nArmor: {unit.Data.Armor}";

        _singleSelectionStatsText.text = text;
    }
    public void ShowUnitList(IReadOnlyList<Unit> units, System.Action<Unit> onClick)
    {
        var count = units != null ? units.Count : 0;

        if (_lastSelectionCount == count && count != 1)
            return;

        _lastSelectionCount = count;
        _lastSingleUnit = null;

        if (_singleSelectionPanel != null)
            _singleSelectionPanel.SetActive(false);

        ClearSelectionIcons();

        if (units == null)
            return;

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];

            if (unit == null)
                continue;

            var icon = Instantiate(_selectionIconPrefab, _selectionIconRoot);
            icon.Initialize(unit, onClick);
            _selectionIcons.Add(icon);
        }
    }

    public void SetCommands(IReadOnlyList<RTSCommandDefinition> commands, System.Action<RTSCommandDefinition> onClick)
    {
        ClearCommandButtons();

        for (int i = 0; i < commands.Count; i++)
        {
            var command = commands[i];

            if (command == null)
                continue;

            var button = Instantiate(_commandButtonPrefab, _commandButtonRoot);
            button.Initialize(command, onClick);
            button.SetActiveVisual(_activeCommandType.HasValue && _activeCommandType.Value == command.Type);
            _commandButtons.Add(button);
        }
    }
    public void SetActiveCommand(RTSCommandType? commandType)
    {
        _activeCommandType = commandType;

        for (int i = 0; i < _commandButtons.Count; i++)
        {
            if (_commandButtons[i] == null)
                continue;

            var active = commandType.HasValue && _commandButtons[i].CommandType == commandType.Value;
            _commandButtons[i].SetActiveVisual(active);
        }
    }

    public void FlashCommand(RTSCommandType commandType)
    {
        for (int i = 0; i < _commandButtons.Count; i++)
        {
            if (_commandButtons[i] == null)
                continue;

            if (_commandButtons[i].CommandType == commandType)
                _commandButtons[i].Flash();
        }
    }
    private string CreateCommandSignature(IReadOnlyList<RTSCommandDefinition> commands)
    {
        if (commands == null || commands.Count == 0)
            return string.Empty;

        var signature = string.Empty;

        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i] == null)
                continue;

            signature += commands[i].Type.ToString();
            signature += "|";
        }

        return signature;
    }

    private void ClearSelectionIcons()
    {
        for (int i = 0; i < _selectionIcons.Count; i++)
        {
            if (_selectionIcons[i] != null)
                Destroy(_selectionIcons[i].gameObject);
        }

        _selectionIcons.Clear();
    }

    private void ClearCommandButtons()
    {
        for (int i = 0; i < _commandButtons.Count; i++)
        {
            if (_commandButtons[i] != null)
                Destroy(_commandButtons[i].gameObject);
        }

        _lastCommandSignature = string.Empty;
        _commandButtons.Clear();
    }
}