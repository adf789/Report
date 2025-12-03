using UnityEngine;

/// <summary>
/// AI 상태 관리 머신
/// </summary>
public class AIStateMachine
{
    private AIState _currentState;

    public AIState CurrentState => _currentState;

    public void Initialize(AIState startState)
    {
        _currentState = startState;
        _currentState?.OnEnter();
    }

    public void Update()
    {
        if (_currentState == null)
            return;

        _currentState.OnUpdate();

        // 상태 전환 체크
        AIState nextState = _currentState.CheckTransition();
        if (nextState != null && nextState != _currentState)
        {
            ChangeState(nextState);
        }
    }

    public void FixedUpdate()
    {
        _currentState?.OnFixedUpdate();
    }

    public void ChangeState(AIState newState)
    {
        if (newState == null || newState == _currentState)
            return;

        _currentState?.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }
}
