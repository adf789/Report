using System;
using UnityEngine;

public class BattleMainUnit : BaseUnit<BattleMainUnitModel>
{
    [SerializeField] private StateBarUnit _playerStateBar;
    [SerializeField] private StateBarUnit _aiPlayerStateBar;
    [SerializeField] private SkillUnit[] _skillUnits;

    public override void Show()
    {
        ShowPlayerStateBar();
        ShowAIPlayerStateBar();
        ShowSkillUnits();
    }

    private void ShowPlayerStateBar()
    {

    }

    private void ShowAIPlayerStateBar()
    {

    }

    private void ShowSkillUnits()
    {
        for (int i = 0; i < _skillUnits.Length; i++)
        {
            var skillUnitModel = Model.GetSkillUnitModel(i);
            bool isActive = !skillUnitModel.IsNull;

            _skillUnits[i].gameObject.SetActive(isActive);

            if (isActive)
            {
                _skillUnits[i].SetModel(skillUnitModel);
                _skillUnits[i].Show();
            }
        }
    }

    public void OnPointDownMove(bool isLeft)
    {
        if (isLeft)
            Model.OnEventLeftMove?.Invoke();
        else
            Model.OnEventRightMove?.Invoke();
    }

    public void OnPointUpMove(bool isLeft)
    {
        Model.OnEventMoveStop?.Invoke();
    }
}
