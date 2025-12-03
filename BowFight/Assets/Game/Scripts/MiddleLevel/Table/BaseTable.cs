using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTable<T> : BaseTable where T : BaseTableData
{
    [SerializeField] protected List<T> datas = new List<T>();

    private Dictionary<uint, T> dataDic;

    public override void Initialize()
    {
        dataDic = new Dictionary<uint, T>();

        foreach (var data in datas)
        {
            if (data != null && !dataDic.ContainsKey(data.ID))
            {
                dataDic[data.ID] = data;
            }
        }
    }

    public T Get(uint id)
    {
        if (dataDic == null)
            Initialize();

        return dataDic.TryGetValue(id, out var data) ? data : null;
    }

    public IReadOnlyList<T> GetAllDatas()
    {
        return datas;
    }

    public int GetDataCount()
    {
        return datas != null ? datas.Count : 0;
    }
}

public abstract class BaseTable : ScriptableObject
{
    public abstract void Initialize();
}
