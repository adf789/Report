using UnityEngine;

public class AIState_Move : AIState
{
    private float _skillDelay;
    private float _moveDelay;
    private float _elapsedSkillTime;
    private float _elapsedMoveTime;
    private MoveState _moveState;
    private System.Action<MoveState> _onEventSetMove = null;

    public AIState_Move(System.Func<AIStateType, AIState> onEventStateGet) : base(onEventStateGet)
    {
    }

    public override void OnEnter()
    {
        _skillDelay = 2f;
        _moveDelay = 1f;
        _elapsedSkillTime = 0f;
        _elapsedMoveTime = 0f;
        _moveState = MoveState.None;

        SetMoveState();
    }

    public override void OnUpdate()
    {

    }

    public override void OnFixedUpdate()
    {
        _elapsedSkillTime += Time.fixedDeltaTime;
        _elapsedMoveTime += Time.fixedDeltaTime;

        if (_elapsedMoveTime >= _moveDelay)
        {
            _elapsedMoveTime = 0f;
            SetMoveState();
        }

        _onEventSetMove?.Invoke(_moveState);
    }

    public override void OnExit()
    {
        _onEventSetMove?.Invoke(MoveState.None);
    }

    public override AIState CheckTransition()
    {
        bool shouldUseSkill = _elapsedSkillTime >= _skillDelay;

        if (shouldUseSkill)
            return GetState(AIStateType.Skill);

        return null;
    }

    public void SetEventMove(System.Action<MoveState> onEvent)
    {
        _onEventSetMove = onEvent;
    }

    private void SetMoveState()
    {
        _moveState = (MoveState)Random.Range(0, 3);
    }
}
