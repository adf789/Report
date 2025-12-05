using UnityEngine;

public struct DamageUnitModel : IUnitModel
{
    public int Damage { get; private set; }
    public SkillEffectType EffectType { get; private set; }

    public DamageUnitModel(int damage, SkillEffectType effectType)
    {
        Damage = damage;
        EffectType = effectType;
    }
}
