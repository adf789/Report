using System;
using UnityEngine;

public class BattleMainUnitModel : IUnitModel
{
    public Action OnEventRightMove { get; private set; }
    public Action OnEventLeftMove { get; private set; }
    public Action OnEventMoveStop { get; private set; }
    public int SkillCount => _skillUnitModel != null ? _skillUnitModel.Length : 0;

    private SkillUnitModel[] _skillUnitModel = null;

    public void SetSkillDatas(SkillTableData[] skillDatas, Func<int, bool> onEventUseSkill)
    {
        int skillCount = skillDatas != null ? skillDatas.Length : 0;
        _skillUnitModel = new SkillUnitModel[skillCount];

        for (int i = 0; i < skillCount; i++)
        {
            _skillUnitModel[i] = new SkillUnitModel(skillDatas[i], i, onEventUseSkill);
        }
    }

    public void SetEvents(Action onEventRightMove, Action onEventLeftMove, Action onEventMoveStop)
    {
        OnEventRightMove = onEventRightMove;
        OnEventLeftMove = onEventLeftMove;
        OnEventMoveStop = onEventMoveStop;
    }

    public SkillUnitModel GetSkillUnitModel(int index)
    {
        if (SkillCount <= index || index < 0)
            return default;

        return _skillUnitModel[index];
    }
}
