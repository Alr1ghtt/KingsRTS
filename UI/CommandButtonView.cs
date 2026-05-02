using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _hotkeyText;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _pressedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private float _flashDuration = 0.5f;

    private RTSCommandDefinition _command;
    private System.Action<RTSCommandDefinition> _onClick;
    private Coroutine _flashRoutine;

    public RTSCommandType CommandType => _command != null ? _command.Type : default;

    public void Initialize(RTSCommandDefinition command, System.Action<RTSCommandDefinition> onClick)
    {
        _command = command;
        _onClick = onClick;

        if (_icon != null)
        {
            _icon.sprite = command.Icon;
            _icon.enabled = command.Icon != null;
            _icon.color = _normalColor;
        }

        if (_hotkeyText != null)
            _hotkeyText.text = command.Hotkey;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(Click);
        }
    }

    public void SetActiveVisual(bool active)
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }

        if (_icon != null)
            _icon.color = active ? _pressedColor : _normalColor;
    }

    public void Flash()
    {
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (_icon != null)
            _icon.color = _pressedColor;

        yield return new WaitForSeconds(_flashDuration);

        if (_icon != null)
            _icon.color = _normalColor;

        _flashRoutine = null;
    }

    private void Click()
    {
        _onClick?.Invoke(_command);
    }
}