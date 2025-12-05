using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ArcherAnimation : MonoBehaviour
{
    public BowLoadState BowState { get; private set; }
    public float Speed { get => _animator.speed; }

    [Header("Bow")]
    [SerializeField] private LineRenderer _bowstringLine;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _tip01;
    [SerializeField] private Transform _tip02;
    [SerializeField] private Transform _limb01;
    [SerializeField] private Transform _limb02;
    [SerializeField] private Transform _nockPoint;
    [SerializeField] private Transform _bowstringAnchorPoint;
    [SerializeField] private AnimationCurve _bowReleaseCurve;

    [Header("Arrow")]
    [SerializeField] private GameObject _arrowInHand;

    private Vector3 _nockLocalPos;
    private Vector3 _originLimb01Angles;
    private Vector3 _originLimb02Angles;

    private Animator _animator;
    private Action<Vector3> _onEventCreateArrow;
    private CancellationTokenSource _bowAnimation;
    private CancellationTokenSource _reloadAnimation;

    private readonly int ONE_MILLISECOND = 1000;
    private readonly float BOW_BENT_VALUE = 15f;

    void Awake()
    {
        BindAnimationEvents();
    }

    void LateUpdate()
    {
        OnUpdateSetBowstring();
    }

    void OnEnable()
    {
        if (_nockPoint)
        {
            _nockLocalPos = _nockPoint.localPosition;
        }

        if (_limb01 && _limb02)
        {
            _originLimb01Angles = _limb01.localEulerAngles;
            _originLimb02Angles = _limb02.localEulerAngles;
        }
    }

    public void SetFloat(AnimationParameter parameter, float value)
    {
        _animator.SetFloat(parameter.ToString(), value);
    }

    public void SetBool(AnimationParameter parameter, bool value)
    {
        _animator.SetBool(parameter.ToString(), value);
    }

    public void SetTrigger(AnimationParameter parameter)
    {
        _animator.SetTrigger(parameter.ToString());
    }

    public void SetSpeed(float value)
    {
        _animator.speed = value;
    }

    private void BindAnimationEvents()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        foreach (var smb in _animator.GetBehaviours<ArcherBowSMB>())
        {
            smb.OnEventLoadBow += LoadBowAnimation;
            smb.OnEventShootArrow += ShootArrowAnimation;
        }

        foreach (var smb in _animator.GetBehaviours<ArcherGetArrowSMB>())
        {
            smb.OnEventGetArrow += ReloadArrow;
        }
    }

    public void SetEventCreateArrow(Action<Vector3> onEvent)
    {
        _onEventCreateArrow = onEvent;
    }

    public void LoadBowAnimation(float delay, float duration)
    {
        if (CancelBowAnimation())
            _nockPoint.localPosition = _nockLocalPos;

        LoadBowAnimationAsync(delay, duration).Forget();
    }
    public void ShootArrowAnimation(float delay, float duration)
    {
        if (CancelBowAnimation())
            _nockPoint.position = _bowstringAnchorPoint.position;

        ShootArrowAnimationAsync(delay, duration).Forget();
    }

    public void ReloadArrow(float delay)
    {
        CancelReloadAnimation();

        ReloadArrowAnimationAsync(delay).Forget();
    }

    private async UniTask ReloadArrowAnimationAsync(float delay)
    {
        _reloadAnimation = new CancellationTokenSource();
        int delayTime = (int)(delay * ONE_MILLISECOND);
        bool cancel = false;

        if (await UniTask.Delay(delayTime, cancellationToken: _reloadAnimation.Token).SuppressCancellationThrow())
        {
            cancel = true;
        }

        if (!cancel)
            _arrowInHand.SetActive(true);

        _reloadAnimation.Dispose();
        _reloadAnimation = null;
    }

    private async UniTask LoadBowAnimationAsync(float delay, float duration)
    {
        BowState = BowLoadState.Load;

        _bowAnimation = new CancellationTokenSource();
        int delayTime = (int)(delay * ONE_MILLISECOND);

        _arrowInHand.SetActive(true);

        if (await UniTask.Delay(delayTime, cancellationToken: _bowAnimation.Token).SuppressCancellationThrow())
            return;

        // 휜 활의 각도를 가져옴
        var bentBowAngles = GetBentBowAngles();

        _nockPoint.localPosition = _nockLocalPos;

        float t = 0;
        float inverseDuration = duration > 0 ? 1 / duration : 1;

        // 활을 휨
        while (t < 1)
        {
            t += Time.deltaTime * inverseDuration;
            _limb01.localEulerAngles =
            Vector3.Lerp(_originLimb01Angles, bentBowAngles.limb01, t);
            _limb02.localEulerAngles =
            Vector3.Lerp(_originLimb02Angles, bentBowAngles.limb02, t);

            _nockPoint.position = Vector3.Lerp(_nockPoint.position, _bowstringAnchorPoint.position, t);

            if (_bowAnimation == null
            || await UniTask.NextFrame(cancellationToken: _bowAnimation.Token).SuppressCancellationThrow())
                break;
        }

        _bowAnimation?.Dispose();
        _bowAnimation = null;
    }

    private async UniTask ShootArrowAnimationAsync(float delay, float duration)
    {
        BowState = BowLoadState.Shoot;

        _bowAnimation = new CancellationTokenSource();
        int delayTime = (int)(delay * ONE_MILLISECOND);

        if (await UniTask.Delay(delayTime, cancellationToken: _bowAnimation.Token).SuppressCancellationThrow())
            return;

        // 휜 활의 각도를 가져옴
        var bentBowAngles = GetBentBowAngles();
        Vector3 originNockLocalPos = _nockPoint.localPosition;

        // 화살 발사
        if (_firePoint)
            _onEventCreateArrow?.Invoke(_firePoint.position);

        // 손에 있는 화살 비활성화
        _arrowInHand.SetActive(false);

        float t = 0;
        float inverseDuration = duration > 0 ? 1 / duration : 1;

        // 휜 활을 복원
        while (t < 1)
        {
            t += Time.deltaTime * inverseDuration;
            _limb01.localEulerAngles =
            Vector3.LerpUnclamped(bentBowAngles.limb01, _originLimb01Angles, _bowReleaseCurve.Evaluate(t));
            _limb02.localEulerAngles =
            Vector3.LerpUnclamped(bentBowAngles.limb02, _originLimb02Angles, _bowReleaseCurve.Evaluate(t));

            _nockPoint.localPosition = Vector3.LerpUnclamped(originNockLocalPos, _nockLocalPos, _bowReleaseCurve.Evaluate(t));

            if (_bowAnimation == null
                || await UniTask.NextFrame(cancellationToken: _bowAnimation.Token).SuppressCancellationThrow())
                break;
        }

        _bowAnimation?.Dispose();
        _bowAnimation = null;

        BowState = BowLoadState.None;
    }

    public bool CancelBowAnimation()
    {
        if (_bowAnimation == null)
            return false;

        _bowAnimation.Cancel();

        _limb01.localEulerAngles = _originLimb01Angles;
        _limb02.localEulerAngles = _originLimb02Angles;
        _nockPoint.localPosition = _nockLocalPos;

        BowState = BowLoadState.None;

        return true;
    }

    public bool CancelReloadAnimation()
    {
        if (_reloadAnimation == null)
            return false;

        _reloadAnimation.Cancel();

        return true;
    }

    private void OnUpdateSetBowstring()
    {
        if (!_bowstringLine || !_tip01 || !_tip02 || !_nockPoint)
        {
            return;
        }

        _bowstringLine.positionCount = 3;
        _bowstringLine.SetPosition(0, _tip01.position);
        _bowstringLine.SetPosition(1, _nockPoint.position);
        _bowstringLine.SetPosition(2, _tip02.position);
    }

    private (Vector3 limb01, Vector3 limb02) GetBentBowAngles()
    {
        Vector3 limb01LoadAngles =
        new Vector3(_originLimb01Angles.x, _originLimb01Angles.y, _originLimb01Angles.z - BOW_BENT_VALUE);
        Vector3 limb02LoadAngles =
        new Vector3(_originLimb02Angles.x, _originLimb02Angles.y, _originLimb02Angles.z - BOW_BENT_VALUE);

        return (limb01LoadAngles, limb02LoadAngles);
    }
}
