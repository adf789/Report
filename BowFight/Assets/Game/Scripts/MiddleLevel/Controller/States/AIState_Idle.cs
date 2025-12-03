using UnityEngine;

/// <summary>
/// Idle 상태: 0.5초 대기 후 Move로 전환
/// </summary>
public class AIState_Idle : AIState
{
    private float _idleDuration;
    private float _elapsedTime;

    public AIState_Idle(AIController controller, float duration = 0.5f) : base(controller)
    {
        _idleDuration = duration;
    }

    public override void OnEnter()
    {
        _elapsedTime = 0f;
    }

    public override void OnUpdate()
    {
        _elapsedTime += Time.deltaTime;
    }

    public override void OnExit()
    {

    }

    public override AIState CheckTransition()
    {
        // 0.5초 경과 시 Move 상태로 전환
        if (_elapsedTime >= _idleDuration)
        {
            return _controller.GetState<AIState_Move>();
        }

        return null;
    }
}
