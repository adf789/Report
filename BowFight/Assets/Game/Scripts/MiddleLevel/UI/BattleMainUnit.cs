using System;
using UnityEngine;

public class BattleMainUnit : BaseUnit<BattleMainUnitModel>
{
    [SerializeField] private StateBarUnit _playerStateBar;
    [SerializeField] private StateBarUnit _aiPlayerStateBar;
    [SerializeField] private GameObject _blind;
    [SerializeField] private Transform _blindCircle;
    [SerializeField] private SkillUnit[] _skillUnits;

    private Camera _mainCamera;
    private Camera _viewCamera;

    private Func<Vector3> _onEventBlindTargetPosition = null;

    public override void Show()
    {
        ShowPlayerStateBar();
        ShowAIPlayerStateBar();
        ShowSkillUnits();
    }

    void LateUpdate()
    {
        if (_blind.activeSelf)
        {
            var screenPoint = _mainCamera.WorldToScreenPoint(GetBlindTargetPosition());

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, screenPoint, _viewCamera, out var localPos))
                _blindCircle.localPosition = localPos;
        }
    }

    public void ShowPlayerStateBar()
    {
        _playerStateBar.SetModel(Model.PlayerStateBarModel);

        _playerStateBar.Show();
    }

    public void ShowAIPlayerStateBar()
    {
        _aiPlayerStateBar.SetModel(Model.AIPlayerStateBarModel);

        _aiPlayerStateBar.Show();
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

    public void SetCamera(Camera main, Camera view)
    {
        _mainCamera = main;
        _viewCamera = view;
    }

    public void SetEventBlindTargetPosition(Func<Vector3> onEvent)
    {
        _onEventBlindTargetPosition = onEvent;
    }

    public void SetActiveBlind(bool isActive)
    {
        _blind.SetActive(isActive);
    }

    private Vector3 GetBlindTargetPosition()
    {
        if (_onEventBlindTargetPosition == null)
            return Vector3.zero;

        return _onEventBlindTargetPosition();
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
