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
        if (_isPause)
            return;

        _archer.Move(MoveState.BackwardMove);
    }

    public void RightMove()
    {
        if (_isPause)
            return;

        _archer.Move(MoveState.ForwardMove);
    }

    public void StopMove()
    {
        _archer.Move(MoveState.None);
    }

    public bool UseSkill(int index)
    {
        if (_isPause)
            return false;

        return ShootSkillArrow(index);
    }
}
