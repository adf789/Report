using UnityEngine;

public class AIState_Idle : AIState
{
    private float _idleDuration;
    private float _elapsedTime;

    public AIState_Idle(System.Func<AIStateType, AIState> onEventStateGet, float duration = 0.5f) : base(onEventStateGet)
    {
        _idleDuration = duration;
    }

    public override void OnEnter()
    {
        _elapsedTime = 0f;
    }

    public override void OnFixedUpdate()
    {
        _elapsedTime += Time.fixedDeltaTime;
    }


    public override void OnExit()
    {

    }

    public override AIState CheckTransition()
    {
        if (_elapsedTime >= _idleDuration)
        {
            return GetState(AIStateType.Move);
        }

        return null;
    }
}
