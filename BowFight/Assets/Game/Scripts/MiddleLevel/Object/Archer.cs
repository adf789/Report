using System;
using System.Collections;
using UnityEngine;

public class Archer : MonoBehaviour, ITarget
{
    public MoveState MoveState => _moveState;
    public float MoveSpeed => _moveSpeed;
    public float DecelerationSpeed => _decelerationSpeed;
    public bool IsFlip => _isFlip;
    public int InstanceID => gameObject.GetInstanceID();

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _targetCollider;

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

    [Header("Values")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _decelerationSpeed = 5f;
    [SerializeField] private bool _isFlip = false;

    private MoveState _moveState;
    private float _moveDirection;

    private ITarget _target;
    private ArrowObjectPool _arrowPool = null;
    private Vector3 _nockPointRestLocalPosition;
    private Vector3 _initialLimb01LocalEulerAngles;
    private Vector3 _initialLimb02LocalEulerAngles;

    private IEnumerator _bowAnimation;
    private IEnumerator _getArrowAnimation;

    void Awake()
    {
        BindAnimationEvents();
    }

    void Update()
    {
        OnUpdateMove();
    }

    void LateUpdate()
    {
        OnUpdateSetBowstring();
    }

    void OnEnable()
    {
        if (_nockPoint)
        {
            _nockPointRestLocalPosition = _nockPoint.localPosition;
        }

        if (_limb01 && _limb02)
        {
            _initialLimb01LocalEulerAngles = _limb01.localEulerAngles;
            _initialLimb02LocalEulerAngles = _limb02.localEulerAngles;
        }
    }

    private void BindAnimationEvents()
    {
        foreach (var smb in _animator.GetBehaviours<ArcherBowSMB>())
        {
            smb.OnEventLoadBow += LoadBow;
            smb.OnEventShootArrow += ShootArrow;
        }

        foreach (var smb in _animator.GetBehaviours<ArcherGetArrowSMB>())
        {
            smb.OnEventGetArrow += ReloadArrow;
        }
    }

    public void Fire()
    {
        _animator.SetTrigger("Shoot");
    }

    public void DirectFire()
    {
        var arrow = GetArrow();

        arrow?.StartMove(ArrowMoveType.Direct);
    }

    public void Move(MoveState state)
    {
        _moveState = state;
    }

    public void Jump()
    {
        _animator.SetBool(AnimationState.Jump.ToString(), true);
    }

    public void JumpLand()
    {
        _animator.SetBool(AnimationState.Jump.ToString(), false);
    }

    public void SetArrowPool(ArrowObjectPool arrowPool)
    {
        _arrowPool = arrowPool;
    }

    public void SetTarget(ITarget target)
    {
        _target = target;
    }

    public void OnDamaged(Arrow arrow)
    {
        Debug.Log("Damaged!");
    }

    public Vector3 GetPosition()
    {
        return _targetCollider ? _targetCollider.bounds.center : transform.position;
    }

    public void LoadBow(float delay, float duration)
    {
        if (_bowAnimation != null)
        {
            StopCoroutine(_bowAnimation);
            _nockPoint.localPosition = _nockPointRestLocalPosition;
        }
        _bowAnimation = LoadBowCoroutine(delay, duration);
        StartCoroutine(_bowAnimation);
    }
    public void ShootArrow(float delay, float duration)
    {
        if (_bowAnimation != null)
        {
            StopCoroutine(_bowAnimation);
            _nockPoint.position = _bowstringAnchorPoint.position;
        }
        _bowAnimation = ShootArrowCoroutine(delay, duration);
        StartCoroutine(_bowAnimation);
    }

    public void ReloadArrow(float delay)
    {
        if (_getArrowAnimation != null)
        {
            StopCoroutine(_getArrowAnimation);
        }
        _getArrowAnimation = GetArrowCoroutine(delay);
        StartCoroutine(_getArrowAnimation);
    }

    private IEnumerator GetArrowCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        _arrowInHand.SetActive(true);
    }

    private IEnumerator LoadBowCoroutine(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        Vector3 limb01LoadLocalEulerAngles =
        new Vector3(_initialLimb01LocalEulerAngles.x, _initialLimb01LocalEulerAngles.y, _initialLimb01LocalEulerAngles.z - 15f);
        Vector3 limb02LoadLocalEulerAngles =
        new Vector3(_initialLimb02LocalEulerAngles.x, _initialLimb02LocalEulerAngles.y, _initialLimb02LocalEulerAngles.z - 15f);

        _nockPoint.localPosition = _nockPointRestLocalPosition;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            _limb01.localEulerAngles =
            Vector3.Lerp(_initialLimb01LocalEulerAngles, limb01LoadLocalEulerAngles, t);
            _limb02.localEulerAngles =
            Vector3.Lerp(_initialLimb02LocalEulerAngles, limb02LoadLocalEulerAngles, t);

            _nockPoint.position = Vector3.Lerp(_nockPoint.position, _bowstringAnchorPoint.position, t);

            yield return null;
        }
    }

    private IEnumerator ShootArrowCoroutine(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        Vector3 limb01LoadLocalEulerAngles =
        new Vector3(_initialLimb01LocalEulerAngles.x, _initialLimb01LocalEulerAngles.y, _initialLimb01LocalEulerAngles.z - 15f);
        Vector3 limb02LoadLocalEulerAngles =
        new Vector3(_initialLimb02LocalEulerAngles.x, _initialLimb02LocalEulerAngles.y, _initialLimb02LocalEulerAngles.z - 15f);

        Vector3 initialNockRestLocalPosition = _nockPoint.localPosition;

        _arrowInHand.SetActive(false);

        // 화살 발사+
        var arrow = GetArrow();
        arrow?.StartMove(ArrowMoveType.Parabola);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            _limb01.localEulerAngles =
            Vector3.LerpUnclamped(limb01LoadLocalEulerAngles, _initialLimb01LocalEulerAngles, _bowReleaseCurve.Evaluate(t));
            _limb02.localEulerAngles =
            Vector3.LerpUnclamped(limb02LoadLocalEulerAngles, _initialLimb02LocalEulerAngles, _bowReleaseCurve.Evaluate(t));

            _nockPoint.localPosition = Vector3.LerpUnclamped(initialNockRestLocalPosition, _nockPointRestLocalPosition, _bowReleaseCurve.Evaluate(t));

            yield return null;
        }
    }

    private void OnUpdateMove()
    {
        // 방향 전환 유무
        int flip = _isFlip ? -1 : 1;

        switch (_moveState)
        {
            case MoveState.LeftMove:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, -1f * flip, _decelerationSpeed * Time.deltaTime);
                }
                break;

            case MoveState.RightMove:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, 1f * flip, _decelerationSpeed * Time.deltaTime);
                }
                break;

            case MoveState.None:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, 0f, _decelerationSpeed * Time.deltaTime);
                }
                break;
        }

        _animator.SetFloat("MoveDirection", _moveDirection);
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

    private void ArrowReturnToPool(Arrow arrow)
    {
        _arrowPool?.Add(arrow);
    }

    private Arrow GetArrow()
    {
        if (!_arrowPool)
        {
            Debug.LogError("Arrow pool is null");
            return null;
        }

        if (_target == null)
        {
            Debug.LogError("Target is null");
            return null;
        }

        var arrow = _arrowPool.Get();

        arrow.SetEventReturnToPool(ArrowReturnToPool);
        arrow.SetPosition(_firePoint.transform.position);
        arrow.SetTarget(_target);

        return arrow;
    }
}
