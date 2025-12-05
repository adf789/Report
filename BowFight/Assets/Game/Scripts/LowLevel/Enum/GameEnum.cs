public enum AnimationParameter
{
    Jump,
    Shoot,
    SkillShoot,
    SkillShootReady,
    MoveDirection
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
    ForwardMove,
    BackwardMove,
    ForwardDash,
    Shock
}

[System.Flags]
public enum CrowdControlState
{
    None = 1 << 0,
    Shock = 1 << 1,
    Freeze = 1 << 2
}

public enum ArrowMoveType
{
    Parabola,
    Direct,
    HighDirect
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

public enum SkillMoveType
{
    None,
    Jump,
    Dash
}

public enum SkillTargetType
{
    Target,
    Self
}

public enum SkillEffectType
{
    Damage,
    Fire,
    Poison,
    Ice,
    Lightning,
    Dark,
    Heal
}

public enum BuffType
{
    Burning,
    Poison,
    Freeze,
    Shock,
    Blind,
    Heal
}

public enum AIStateType
{
    Idle,
    Move,
    Skill
}