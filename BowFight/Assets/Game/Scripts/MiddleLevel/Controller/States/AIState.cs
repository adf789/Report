using UnityEngine;

/// <summary>
/// AI 상태의 기본 추상 클래스
/// </summary>
public abstract class AIState
{
    protected AIController _controller;

    public AIState(AIController controller)
    {
        _controller = controller;
    }

    /// <summary>
    /// 상태 진입 시 호출
    /// </summary>
    public virtual void OnEnter() { }

    /// <summary>
    /// 상태 실행 중 매 프레임 호출
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// 물리 업데이트 시 호출
    /// </summary>
    public virtual void OnFixedUpdate() { }

    /// <summary>
    /// 상태 종료 시 호출
    /// </summary>
    public virtual void OnExit() { }

    /// <summary>
    /// 다음 상태로 전환 여부 체크
    /// </summary>
    /// <returns>다음 상태 (null이면 상태 유지)</returns>
    public abstract AIState CheckTransition();
}
