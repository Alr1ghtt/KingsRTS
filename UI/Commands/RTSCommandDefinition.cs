using UnityEngine;

[CreateAssetMenu(menuName = "RTS/UI/Command Definition")]
public class RTSCommandDefinition : ScriptableObject
{
    [SerializeField] private RTSCommandType _type;
    [SerializeField] private string _displayName;
    [SerializeField] private string _hotkey;
    [SerializeField] private Sprite _icon;

    public RTSCommandType Type => _type;
    public string DisplayName => _displayName;
    public string Hotkey => _hotkey;
    public Sprite Icon => _icon;
}