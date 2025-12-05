using UnityEngine;

/// <summary>
/// AI 상태의 기본 추상 클래스
/// </summary>
public abstract class AIState
{
    private System.Func<AIStateType, AIState> _onEventStateGet;

    public AIState(System.Func<AIStateType, AIState> onEventStateGet)
    {
        _onEventStateGet = onEventStateGet;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnExit() { }
    public abstract AIState CheckTransition();
    protected AIState GetState(AIStateType stateType)
    {
        if (_onEventStateGet != null)
            return _onEventStateGet(stateType);
        else
            return null;
    }

}
