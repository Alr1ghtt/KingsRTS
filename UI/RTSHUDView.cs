using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    public Button[] ControlGroupButtons => _controlGroupButtons;

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

    public void ShowSingleUnit(Unit unit)
    {
        ClearSelectionIcons();

        if (_singleSelectionPanel != null)
            _singleSelectionPanel.SetActive(unit != null);

        if (unit == null)
            return;

        if (_singleSelectionIcon != null)
            _singleSelectionIcon.sprite = unit.Icon;

        if (_singleSelectionNameText != null)
            _singleSelectionNameText.text = unit.UnitType.ToString();

        if (_singleSelectionStatsText != null)
            _singleSelectionStatsText.text = $"HP: {Mathf.CeilToInt(unit.Context.CurrentHealth)}/{Mathf.CeilToInt(unit.Data.MaxHealth)}\nDamage: {unit.Data.AttackDamage}\nArmor: {unit.Data.Armor}";
    }

    public void ShowUnitList(IReadOnlyList<Unit> units, System.Action<Unit> onClick)
    {
        if (_singleSelectionPanel != null)
            _singleSelectionPanel.SetActive(false);

        ClearSelectionIcons();

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
            var button = Instantiate(_commandButtonPrefab, _commandButtonRoot);
            button.Initialize(command, onClick);
            _commandButtons.Add(button);
        }
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

        _commandButtons.Clear();
    }
}