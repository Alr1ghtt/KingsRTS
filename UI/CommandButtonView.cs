using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _hotkeyText;
    [SerializeField] private TMP_Text _labelText;

    private RTSCommandDefinition _command;
    private System.Action<RTSCommandDefinition> _onClick;

    public void Initialize(RTSCommandDefinition command, System.Action<RTSCommandDefinition> onClick)
    {
        _command = command;
        _onClick = onClick;

        if (_icon != null)
            _icon.sprite = command.Icon;

        if (_hotkeyText != null)
            _hotkeyText.text = command.Hotkey;

        if (_labelText != null)
            _labelText.text = command.DisplayName;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(Click);
        }
    }

    private void Click()
    {
        _onClick?.Invoke(_command);
    }
}