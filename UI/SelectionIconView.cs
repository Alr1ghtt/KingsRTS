using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectionIconView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _countText;

    private Unit _unit;
    private System.Action<Unit> _onClick;

    public void Initialize(Unit unit, System.Action<Unit> onClick)
    {
        _unit = unit;
        _onClick = onClick;

        if (_icon != null)
            _icon.sprite = unit.Icon;

        if (_countText != null)
            _countText.text = string.Empty;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(Click);
        }
    }

    private void Click()
    {
        if (_unit == null)
            return;

        _onClick?.Invoke(_unit);
    }
}