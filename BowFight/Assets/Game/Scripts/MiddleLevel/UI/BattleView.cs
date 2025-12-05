using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleView : BaseUnit<BattleViewModel>
{
    [Header("캐릭터 상태")]
    [SerializeField] private StateBarUnit _playerStateBar;
    [SerializeField] private StateBarUnit _aiPlayerStateBar;

    [Header("실명 효과")]
    [SerializeField] private GameObject _blind;
    [SerializeField] private Transform _blindCircle;

    [Header("스킬 카드")]
    [SerializeField] private SkillUnit[] _skillUnits;

    [Header("데미지 텍스트")]
    [SerializeField] private DamageObjectPool _damagePool;

    [Header("결과 화면")]
    [SerializeField] private Image _resultScreen;
    [SerializeField] private Button _resultButton;
    [SerializeField] private GameObject _victoryText;
    [SerializeField] private GameObject _defeatText;
    [SerializeField] private TextMeshProUGUI _continueText;

    private Camera _mainCamera;
    private Camera _viewCamera;

    private Func<Vector3> _onEventBlindTargetPosition = null;
    private Action _onEventReturnToIntro = null;
    private bool _isResultOn = false;

    void Start()
    {
        _damagePool.Initialize();
    }

    public override void Show()
    {
        ShowPlayerStateBar();
        ShowAIPlayerStateBar();
        ShowSkillUnits();
        StartActiveResultScreen(false, false);
    }

    void LateUpdate()
    {
        OnUpdateBlind();
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

    public void ShowDamage(Vector3 position, float damage, SkillEffectType effectType)
    {
        var parent = _damagePool.transform as RectTransform;
        var localPos = GetLocalPosition(parent, position);
        var damageUnit = _damagePool.Get();

        damageUnit.transform.localPosition = localPos;
        damageUnit.SetModel(new DamageUnitModel((int)damage, effectType));
        damageUnit.SetEventReturnToPool(OnEventReturnToDamagePool);

        damageUnit.Show();
        damageUnit.SetActive(true);
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

    public void SetEventReturnToIntro(Action onEvent)
    {
        _onEventReturnToIntro = onEvent;
    }

    public void SetActiveBlind(bool isActive)
    {
        _blind.SetActive(isActive);
    }

    public void StartActiveResultScreen(bool isActive, bool isWin)
    {
        if (isActive
        && isActive != _resultScreen.gameObject.activeSelf)
        {
            if (!_isResultOn)
                OnEventActiveResult(isWin).Forget();
            return;
        }

        _resultScreen.gameObject.SetActive(isActive);
    }

    private Vector3 GetBlindTargetPosition()
    {
        if (_onEventBlindTargetPosition == null)
            return Vector3.zero;

        return _onEventBlindTargetPosition();
    }

    private Vector3 GetLocalPosition(RectTransform rectTransform, Vector3 position)
    {
        var screenPoint = _mainCamera.WorldToScreenPoint(position);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, _viewCamera, out var localPos))
            return localPos;

        return Vector3.zero;
    }

    public void OnPointDownMove(bool isLeft)
    {
        if (isLeft)
            Model.OnEventLeftMove?.Invoke();
        else
            Model.OnEventRightMove?.Invoke();
    }

    public void OnPointUpMove()
    {
        Model.OnEventMoveStop?.Invoke();
    }

    public void OnClickReturnToIntro()
    {
        _onEventReturnToIntro?.Invoke();
    }

    private void OnEventReturnToDamagePool(DamageUnit damageUnit)
    {
        _damagePool.Add(damageUnit);
    }

    private void OnUpdateBlind()
    {
        if (!_blind.activeSelf)
            return;

        var targetPosition = GetBlindTargetPosition();
        var parent = transform.parent as RectTransform;
        var localPos = GetLocalPosition(parent, targetPosition);

        _blindCircle.localPosition = localPos;
    }

    private async UniTask OnEventActiveResult(bool isWin)
    {
        // 초기 상태 설정
        _isResultOn = true;
        _victoryText.SetActive(false);
        _defeatText.SetActive(false);
        _continueText.gameObject.SetActive(false);
        _resultButton.enabled = false;

        // 결과 배경화면 Fade-In 애니메이션
        Color resultColor = isWin ? Color.white : Color.black;
        Color continueTextColor = isWin ? Color.black : Color.white;
        resultColor.a = 0;
        _resultScreen.color = resultColor;
        _continueText.color = continueTextColor;

        _resultScreen.gameObject.SetActive(true);

        while (resultColor.a < 1)
        {
            resultColor.a += Time.deltaTime;
            _resultScreen.color = resultColor;

            await UniTask.Yield();
        }

        resultColor.a = 1;
        _resultScreen.color = resultColor;

        // 결과 텍스트 활성화
        if (isWin)
            _victoryText.SetActive(true);
        else
            _defeatText.SetActive(true);

        // 1초 대기
        await UniTask.Delay(1000);

        // 버튼 및 텍스트 활성화
        _continueText.gameObject.SetActive(true);
        _resultButton.enabled = true;
    }
}
