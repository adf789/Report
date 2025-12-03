using System;
using UnityEngine;

public class PlayerController : BaseController
{
    void FixedUpdate()
    {
        OnUpdateMove();
    }

    public void LeftMove()
    {
        _archer.Move(MoveState.LeftMove);
    }

    public void RightMove()
    {
        _archer.Move(MoveState.RightMove);
    }

    public void StopMove()
    {
        _archer.Move(MoveState.None);
    }

    public bool UseSkill(int index)
    {
        return ShootSkillArrow(index);
    }
}
