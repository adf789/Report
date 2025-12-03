using UnityEngine;

public class StateBarUnitModel : IUnitModel
{
    public float MaxHp { get; set; }
    public float CurrentHp { get; set; }
    public float HpRate { get => MaxHp > 0 ? CurrentHp / MaxHp : 0; }
}
