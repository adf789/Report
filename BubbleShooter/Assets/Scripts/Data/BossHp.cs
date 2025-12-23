using System;
using UnityEngine;

[Serializable]
public class BossHp
{
    public int MaxHp { get; private set; }
    public int CurrentHp { get; set; }

    public float Rate => Mathf.Clamp01((float)CurrentHp / MaxHp);
    public bool IsDeath => CurrentHp <= 0;

    public BossHp(int hp)
    {
        MaxHp = Mathf.Max(1, hp);
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// damaged hp, return value is dead check
    /// </summary>
    public bool Damage(int damage)
    {
        if (CurrentHp > damage)
        {
            CurrentHp -= damage;
            return false;
        }
        else
        {
            CurrentHp = 0;
            return true;
        }
    }
}
