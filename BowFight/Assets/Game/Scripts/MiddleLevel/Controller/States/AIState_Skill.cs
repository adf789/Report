using UnityEngine;

/// <summary>
/// Skill 상태: 스킬 사용 후 Idle로 전환
/// </summary>
public class AIState_Skill : AIState
{
    public int SkillCount => _coolTimes != null ? _coolTimes.Length : 0;
    private int _useSkillIndex;
    private bool _skillExecuted;
    private float[] _coolTimes;
    private float[] _useSkillTimes;

    private System.Func<int, bool> _onEventUseSkill = null;

    public AIState_Skill(AIController controller) : base(controller)
    {
    }

    public override void OnEnter()
    {
        _useSkillIndex = -1;
        _skillExecuted = false;
    }

    public override void OnUpdate()
    {
        if (!_skillExecuted)
        {
            _skillExecuted = true;
            _useSkillIndex = SelectSkill();
        }
    }

    public override void OnExit()
    {

    }

    public override AIState CheckTransition()
    {
        // 스킬 완료 후 Idle로 전환
        if (_skillExecuted)
        {
            if (_useSkillIndex >= 0)
                ExecuteSkill(_useSkillIndex);
        }

        return _controller.GetState<AIState_Move>();
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
            }
        }
    }

    public void SetEventUseSkill(System.Func<int, bool> onEvent)
    {
        _onEventUseSkill = onEvent;
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
