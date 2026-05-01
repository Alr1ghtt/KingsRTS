using UnityEngine;

[CreateAssetMenu(menuName = "RTS/UI/Command Library")]
public class RTSCommandLibrary : ScriptableObject
{
    [SerializeField] private RTSCommandDefinition _move;
    [SerializeField] private RTSCommandDefinition _holdPosition;
    [SerializeField] private RTSCommandDefinition _patrol;
    [SerializeField] private RTSCommandDefinition _attack;
    [SerializeField] private RTSCommandDefinition _heal;
    [SerializeField] private RTSCommandDefinition _repair;
    [SerializeField] private RTSCommandDefinition _buildMenu;
    [SerializeField] private RTSCommandDefinition _buildArchery;
    [SerializeField] private RTSCommandDefinition _buildBarracks;
    [SerializeField] private RTSCommandDefinition _buildCastle;
    [SerializeField] private RTSCommandDefinition _buildHouse;
    [SerializeField] private RTSCommandDefinition _buildMonastery;
    [SerializeField] private RTSCommandDefinition _buildTower;

    public RTSCommandDefinition Move => _move;
    public RTSCommandDefinition HoldPosition => _holdPosition;
    public RTSCommandDefinition Patrol => _patrol;
    public RTSCommandDefinition Attack => _attack;
    public RTSCommandDefinition Heal => _heal;
    public RTSCommandDefinition Repair => _repair;
    public RTSCommandDefinition BuildMenu => _buildMenu;
    public RTSCommandDefinition BuildArchery => _buildArchery;
    public RTSCommandDefinition BuildBarracks => _buildBarracks;
    public RTSCommandDefinition BuildCastle => _buildCastle;
    public RTSCommandDefinition BuildHouse => _buildHouse;
    public RTSCommandDefinition BuildMonastery => _buildMonastery;
    public RTSCommandDefinition BuildTower => _buildTower;
}