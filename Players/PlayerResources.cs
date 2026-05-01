using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private int _playerId;
    [SerializeField] private int _food;
    [SerializeField] private int _gold;
    [SerializeField] private int _wood;
    [SerializeField] private int _combatLimit;
    [SerializeField] private int _combatUsed;
    [SerializeField] private int _workerCount;

    public int PlayerId => _playerId;
    public int Food => _food;
    public int Gold => _gold;
    public int Wood => _wood;
    public int CombatLimit => _combatLimit;
    public int CombatUsed => _combatUsed;
    public int WorkerCount => _workerCount;

    public void SetFood(int value)
    {
        _food = Mathf.Max(0, value);
    }

    public void SetGold(int value)
    {
        _gold = Mathf.Max(0, value);
    }

    public void SetWood(int value)
    {
        _wood = Mathf.Max(0, value);
    }

    public void SetCombatLimit(int value)
    {
        _combatLimit = Mathf.Max(0, value);
    }

    public void SetCombatUsed(int value)
    {
        _combatUsed = Mathf.Max(0, value);
    }

    public void SetWorkerCount(int value)
    {
        _workerCount = Mathf.Max(0, value);
    }
}