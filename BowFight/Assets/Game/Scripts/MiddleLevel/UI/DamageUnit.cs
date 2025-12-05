using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DamageUnit : BaseUnit<DamageUnitModel>, IReusable
{
    [SerializeField] private TextMeshProUGUI _damageText;

    [Header("Animation Settings")]
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _minHeight = 50f;
    [SerializeField] private float _maxHeight = 100f;
    [SerializeField] private float _horizontalRange = 30f;
    [SerializeField] private AnimationCurve _heightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.2f);
    [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private System.Action<DamageUnit> _onEventReturnToPool;
    private CancellationTokenSource _animation;

    private void Awake()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public override void Show()
    {
        ShowDamage();

        StopAnimation();
        PlayAnimation().Forget();
    }

    void OnDisable()
    {
        StopAnimation();
    }

    private void ShowDamage()
    {
        _damageText.text = Model.Damage.ToString("n0");

        switch (Model.EffectType)
        {
            case SkillEffectType.Fire:
                _damageText.color = Color.red;
                break;

            case SkillEffectType.Poison:
                _damageText.color = Color.green;
                break;

            case SkillEffectType.Ice:
                _damageText.color = Color.cyan;
                break;

            case SkillEffectType.Lightning:
                _damageText.color = Color.yellow;
                break;

            case SkillEffectType.Dark:
                _damageText.color = Color.gray;
                break;

            case SkillEffectType.Heal:
                _damageText.color = Color.magenta;
                break;

            default:
                _damageText.color = Color.white;
                break;
        }
    }

    private async UniTaskVoid PlayAnimation()
    {
        _animation = new CancellationTokenSource();

        // 랜덤 방향과 높이 설정
        float randomHeight = Random.Range(_minHeight, _maxHeight);
        float randomX = Random.Range(-_horizontalRange, _horizontalRange);

        Vector2 startPos = _rectTransform.anchoredPosition;

        // 초기 상태 설정
        _canvasGroup.alpha = 1f;

        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _duration;

            // 포물선 움직임 (높이)
            float heightValue = _heightCurve.Evaluate(t);

            // 수평 이동 (선형)
            Vector2 currentPos = startPos + new Vector2(
                randomX * t,
                randomHeight * heightValue
            );

            _rectTransform.anchoredPosition = currentPos;

            // 페이드 아웃
            _canvasGroup.alpha = _alphaCurve.Evaluate(t);

            if (await UniTask.Yield(cancellationToken: _animation.Token).SuppressCancellationThrow())
                break;
        }

        // 애니메이션 종료 후 비활성화
        if (this)
        {
            gameObject.SetActive(false);
            _animation = null;
        }
    }

    private void StopAnimation()
    {
        if (_animation != null)
        {
            _animation.Cancel();
            _animation.Dispose();
            _animation = null;
        }
    }

    public void SetEventReturnToPool(System.Action<DamageUnit> onEvent)
    {
        _onEventReturnToPool = onEvent;
    }

    public void Initialize()
    {

    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void ReturnToPool()
    {
        _onEventReturnToPool?.Invoke(this);
    }

    public void ResetEvents()
    {
        _onEventReturnToPool = null;
    }
}
