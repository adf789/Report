using UnityEngine;

public struct DamageUnitModel : IUnitModel
{
    public int Damage { get; private set; }

    public DamageUnitModel(int damage)
    {
        Damage = damage;
    }
}
