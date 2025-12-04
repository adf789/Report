public enum AnimationState
{
    Idle,
    Move,
    Jump,
    Shoot,
    SkillShoot,
    SkillShootReady
}

public enum BowLoadState
{
    None,
    Load,
    Shoot
}

public enum MoveState
{
    None,
    LeftMove,
    RightMove
}

public enum ArrowMoveType
{
    Parabola,
    Direct
}

public enum BowConditionState
{
    OnEnter,
    OnExit
}

public enum BowActionType
{
    Pull,
    Release,
    Cancel
}