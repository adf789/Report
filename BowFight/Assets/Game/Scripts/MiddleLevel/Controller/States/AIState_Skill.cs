using UnityEngine;

public class AIState_Skill : AIState
{
    public int SkillCount => _coolTimes != null ? _coolTimes.Length : 0;
    private int _useSkillIndex;
    private float[] _coolTimes;
    private float[] _useSkillTimes;

    private System.Func<int, bool> _onEventUseSkill = null;
    private System.Func<bool> _onEventHasLoadSkill = null;

    public AIState_Skill(System.Func<AIStateType, AIState> onEventStateGet) : base(onEventStateGet)
    {
    }

    public override void OnEnter()
    {
        _useSkillIndex = SelectSkill();

        if (_onEventHasLoadSkill == null)
            Debug.LogError("AI 스킬 종료 이벤트가 등록되어 있지 않습니다.");
    }

    public override void OnExit()
    {

    }

    public override AIState CheckTransition()
    {
        if (_useSkillIndex >= 0)
        {
            ExecuteSkill(_useSkillIndex);
            _useSkillIndex = -1;
        }

        if (_onEventHasLoadSkill != null & !_onEventHasLoadSkill())
            return GetState(AIStateType.Move);

        return null;
    }

    public void SetSkillData(SkillTableData[] skillDatas)
    {
        int skillCount = skillDatas != null ? skillDatas.Length : 0;

        _coolTimes = new float[skillCount];
        _useSkillTimes = new float[skillCount];

        for (int i = 0; i < skillCount; i++)
        {
            var skillData = skillDatas[i];

            if (skillData == null)
            {
                _coolTimes[i] = -1;
            }
            else
            {
                _coolTimes[i] = skillData.CoolTime;
                _useSkillTimes[i] = Time.realtimeSinceStartup;
            }
        }
    }

    public void SetEventUseSkill(System.Func<int, bool> onEvent)
    {
        _onEventUseSkill = onEvent;
    }

    public void SetEventHasLoadSkill(System.Func<bool> onEvent)
    {
        _onEventHasLoadSkill = onEvent;
    }

    private void ExecuteSkill(int index)
    {
        if (SkillCount <= index || index < 0)
            return;

        if (_coolTimes[index] == -1)
            return;

        _useSkillTimes[index] = Time.realtimeSinceStartup;

        _onEventUseSkill?.Invoke(_useSkillIndex);
    }

    private int SelectSkill()
    {
        int randomValue = 0;
        int selectIndex = -1;

        for (int i = 0; i < SkillCount; i++)
        {
            if (_coolTimes[i] == -1)
                continue;

            if (!CheckSkillCoolTime(i)
                && Random.Range(0, ++randomValue) == 0)
                selectIndex = i;
        }

        return selectIndex;
    }

    private bool CheckSkillCoolTime(int index)
    {
        if (SkillCount <= index || index < 0)
            return true;

        if (_coolTimes[index] == -1)
            return true;

        return _useSkillTimes[index] + _coolTimes[index] > Time.realtimeSinceStartup;
    }
}
