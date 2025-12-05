using System.Collections.Generic;
using UnityEngine;

public class AIController : BaseController
{
    [Header("AI Settings")]
    [SerializeField] private float _idleDuration = 0.5f;

    private AIStateMachine _stateMachine;
    private Dictionary<System.Type, AIState> _states;

    public override void Initialize(SkillTableData[] skillDatas)
    {
        base.Initialize(skillDatas);

        InitializeStateMachine();
    }

    private void Update()
    {
        if (!_isPause)
            _stateMachine?.Update();
    }

    private void FixedUpdate()
    {
        if (!_isPause)
            _stateMachine?.FixedUpdate();

        OnUpdateMove();
    }

    /// <summary>
    /// FSM 초기화 및 모든 상태 생성
    /// </summary>
    private void InitializeStateMachine()
    {
        _stateMachine = new AIStateMachine();
        _states = new Dictionary<System.Type, AIState>();

        // 모든 상태 생성 및 등록
        RegisterState(new AIState_Idle(GetState, _idleDuration));
        RegisterState(new AIState_Move(GetState));
        RegisterState(new AIState_Skill(GetState));

        // 상태 머신 시작 (Idle부터 시작)
        _stateMachine.Initialize(GetState<AIState_Idle>());
    }

    /// <summary>
    /// 상태 등록
    /// </summary>
    private void RegisterState(AIState state)
    {
        System.Type stateType = state.GetType();
        if (!_states.ContainsKey(stateType))
        {
            _states.Add(stateType, state);

            OnRegisteredState(state);
        }
    }

    private void OnRegisteredState(AIState state)
    {
        switch (state)
        {
            case AIState_Move moveState:
                {
                    moveState.SetEventMove(SetMove);
                }
                break;

            case AIState_Skill skillState:
                {
                    skillState.SetEventUseSkill(ShootSkillArrow);
                    skillState.SetEventHasLoadSkill(CheckHasLoadSkill);
                    skillState.SetSkillData(_skillDatas);
                }
                break;
        }
    }

    private T GetState<T>() where T : AIState
    {
        System.Type stateType = typeof(T);
        if (_states.TryGetValue(stateType, out AIState state))
        {
            return state as T;
        }

        Debug.LogError($"[AIController] State not found: {stateType.Name}");
        return null;
    }

    private AIState GetState(AIStateType stateType)
    {
        return stateType switch
        {
            AIStateType.Idle => GetState<AIState_Idle>(),
            AIStateType.Move => GetState<AIState_Move>(),
            AIStateType.Skill => GetState<AIState_Skill>(),
            _ => GetState<AIState_Idle>(),
        };
    }

    private void SetMove(MoveState direction)
    {
        _archer?.Move(direction);
    }

    private bool CheckHasLoadSkill()
    {
        if (_archer == null)
            return false;

        return _archer.LoadSkillCount > 0;
    }
}
