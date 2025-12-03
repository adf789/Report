using UnityEngine;

public abstract class BaseTableData : ScriptableObject
{
    public uint ID => _id;
    public string Name => _name;

    [Header("Basic Info")]
    [SerializeField] protected uint _id;
    [SerializeField] protected string _name;

    public void SetID(uint id)
    {
        _id = id;
    }

    public void SetName(string name)
    {
        _name = name;
    }
}