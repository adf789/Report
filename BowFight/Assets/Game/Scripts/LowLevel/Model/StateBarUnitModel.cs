using System.Collections.Generic;
using UnityEngine;

public class StateBarUnitModel : IUnitModel
{
    public float MaxHp { get; set; }
    public float CurrentHp { get; set; }
    public float HpRate { get => MaxHp > 0 ? CurrentHp / MaxHp : 0; }
    public int BuffCount { get => _buffThumbnails.Count; }
    private List<string> _buffThumbnails = new List<string>();

    public void ClearAffectedBuffs()
    {
        _buffThumbnails.Clear();
    }

    public void AddAffectedBuff(string buffThumbnail)
    {
        _buffThumbnails.Add(buffThumbnail);
    }

    public BuffUnitModel GetBuffUnitModel(int index)
    {
        if (BuffCount <= index || index < 0)
            return default;

        return new BuffUnitModel(_buffThumbnails[index]);
    }
}
