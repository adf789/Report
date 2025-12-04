using UnityEngine;

public abstract class BaseTableData : ScriptableObject
{
    public bool IsActive => _isActive;
    public uint ID => _id;
    public string Name => _name;
    public string Thumbnail => _thumbnail;

    [Header("Basic Info")]
    [SerializeField] protected bool _isActive = true;
    [SerializeField] protected uint _id;
    [SerializeField] protected string _name;
    [SerializeField] protected string _thumbnail;

    public void SetID(uint id)
    {
        _id = id;
    }

    public void SetName(string name)
    {
        _name = name;
    }
}