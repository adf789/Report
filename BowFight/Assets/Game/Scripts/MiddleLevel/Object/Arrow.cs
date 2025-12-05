using UnityEngine;

public class Arrow : MonoBehaviour, IReusable
{
    public ArrowMoveType MoveType => GetSkillData().ArrowMoveType;
    public SkillTableData SkillData => _skillData;
    public float TravelTime => _travelTime / GetSkillData().ArrowMoveSpeed;
    public float Damage => _damage;

    [Header("References")]
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private MeshRenderer _renderer;

    [Header("Common Move Setting")]
    [Range(0.5f, 2f)]
    [SerializeField] private float _travelTime = 1f;

    [Header("Parabola Move Setting")]
    [SerializeField] private float _arcHeight = 2f;

    private Vector3 _startPosition;
    private Vector3 _middlePosition;
    private Vector3 _endPosition;
    private float _currentTime;
    private float _inverseTravelTime;
    private float _damage;
    private bool _isMoving = false;
    private bool _isFadeOut = false;
    private SkillTableData _skillData;
    private ITarget _target;
    private System.Action<Arrow> _onEventReturnToPool = null;

    public void Initialize()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void ReturnToPool()
    {
        _isMoving = false;

        _onEventReturnToPool?.Invoke(this);
    }

    public void ResetEvents()
    {
        _onEventReturnToPool = null;
    }

    public void SetPosition(Vector3 start)
    {
        _startPosition = transform.position = start;
    }

    public void SetTarget(ITarget target)
    {
        _target = target;

        if (_target != null)
            _endPosition = target.GetPosition();
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    public void SetEventReturnToPool(System.Action<Arrow> onEvent)
    {
        _onEventReturnToPool = onEvent;
    }

    public void StartMove(SkillTableData skillData)
    {
        if (_target == null)
            return;

        _currentTime = 0f;
        _isMoving = true;
        _isFadeOut = false;
        _skillData = skillData;
        _inverseTravelTime = 1 / TravelTime;
        _collider.enabled = true;
        _trailRenderer.enabled = true;

        SetMiddlePosition();

        SetArrowType(skillData);

        SetAlpha(1f);

        SetActive(true);
    }

    private void StartFadeOut()
    {
        _currentTime = 0f;
        _isMoving = false;
        _isFadeOut = true;
        _collider.enabled = false;
        _trailRenderer.enabled = false;
    }

    private void SetMiddlePosition()
    {
        // 중간 경로가 있는 경우
        if (MoveType == ArrowMoveType.HighDirect)
        {
            _middlePosition = _startPosition + Vector3.up * 7f;
        }
        else
        {
            _middlePosition = Vector3.zero;
        }
    }

    private void SetArrowType(SkillTableData skillData)
    {
        if (skillData == null)
        {
            _trailRenderer.startColor = Color.white;
            _trailRenderer.endColor = Color.white;
        }
        else
        {
            switch (skillData.EffectType)
            {
                case SkillEffectType.Damage:
                    _trailRenderer.startColor = Color.white;
                    _trailRenderer.endColor = Color.white;
                    break;

                case SkillEffectType.Fire:
                    _trailRenderer.startColor = Color.red;
                    _trailRenderer.endColor = Color.yellow;
                    break;

                case SkillEffectType.Poison:
                    _trailRenderer.startColor = Color.green;
                    _trailRenderer.endColor = Color.clear;
                    break;

                case SkillEffectType.Ice:
                    _trailRenderer.startColor = Color.blue;
                    _trailRenderer.endColor = Color.cyan;
                    break;

                case SkillEffectType.Lightning:
                    _trailRenderer.startColor = Color.yellow;
                    _trailRenderer.endColor = Color.white;
                    break;

                case SkillEffectType.Dark:
                    _trailRenderer.startColor = Color.black;
                    _trailRenderer.endColor = Color.gray;
                    break;

                case SkillEffectType.Heal:
                    _trailRenderer.startColor = Color.magenta;
                    _trailRenderer.endColor = Color.white;
                    break;
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        var color = _renderer.material.color;
        color.a = alpha;
        _renderer.material.color = color;
    }

    private void Update()
    {
        OnUpdateFadeOut();
    }

    private void FixedUpdate()
    {
        OnUpdateArrowTranjectory();
    }

    private void OnUpdateArrowTranjectory()
    {
        if (!_isMoving || _isFadeOut)
            return;

        bool isEnd = true;
        _currentTime += Time.fixedDeltaTime;

        switch (MoveType)
        {
            case ArrowMoveType.Direct:
                {
                    isEnd = DirectMove();
                }
                break;

            case ArrowMoveType.Parabola:
                {
                    isEnd = BesierMove();
                }
                break;

            case ArrowMoveType.HighDirect:
                {
                    isEnd = HighDirectMove();
                }
                break;
        }

        if (isEnd)
            ReturnToPool();
    }

    private void OnUpdateFadeOut()
    {
        if (!_isFadeOut)
            return;

        // 진행도 계산
        _currentTime += Time.deltaTime * 1.5f;

        SetAlpha(1 - _currentTime);

        if (_currentTime >= 1f)
        {
            _isFadeOut = false;
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_target == null)
            return;

        if (other.gameObject.GetInstanceID() == _target.InstanceID)
        {
            StartFadeOut();

            _target.OnDamaged(this);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StartFadeOut();
        }
    }

    private bool DirectMove()
    {
        // 진행도 계산 (0~1)
        float t = _currentTime * _inverseTravelTime;

        // 타임아웃 체크
        if (t >= 2f) return true;

        LookAtTarget(in _endPosition);

        transform.position = _startPosition + (_endPosition - _startPosition) * t;

        return false;
    }

    /// <summary>
    /// 베지어 곡선을 활용하여 포물선을 구현합니다.
    /// </summary>
    /// <returns>도착하면 true, 진행 중이면 false</returns>
    private bool BesierMove()
    {
        // 진행도 계산 (0~1)
        float t = _currentTime * _inverseTravelTime;

        // 타임아웃 체크
        if (t >= 2f) return true;

        // 제어점 계산 (중간점 + 위쪽 오프셋)
        float midX = (_startPosition.x + _endPosition.x) * 0.5f;
        float midY = (_startPosition.y + _endPosition.y) * 0.5f + _arcHeight;

        // 베지어 곡선: (1-t)²P0 + 2(1-t)tP1 + t²P2
        float inv = 1f - t;
        float inv2 = inv * inv;
        float t2 = t * t;
        float blend = 2f * inv * t;

        // 위치 계산
        transform.position = new Vector3(
            inv2 * _startPosition.x + blend * midX + t2 * _endPosition.x,
            inv2 * _startPosition.y + blend * midY + t2 * _endPosition.y,
            0f
        );

        // 회전 계산 (접선 벡터의 간소화)
        float dx = (midX - _startPosition.x) * inv + (_endPosition.x - midX) * t;
        float dy = (midY - _startPosition.y) * inv + (_endPosition.y - midY) * t;

        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        return false;
    }

    private bool HighDirectMove()
    {
        // 진행도 계산 (0~1)
        float t = _currentTime * _inverseTravelTime;

        // 타임아웃 체크
        if (t >= 2f) return true;

        if (t < 0.6f)
        {
            float normalizedT = t / 0.6f;

            LookAtTarget(in _middlePosition);
            transform.position = _startPosition + (_middlePosition - _startPosition) * normalizedT;
        }
        else
        {
            float normalizedT = (t - 0.6f) / 0.4f;

            LookAtTarget(in _endPosition);
            transform.position = _middlePosition + (_endPosition - _middlePosition) * normalizedT;
        }

        return false;
    }

    private void LookAtTarget(in Vector3 targetPosition)
    {
        // 타겟 방향으로 화살표 회전
        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private SkillTableData GetSkillData()
    {
        return _skillData != null ? _skillData : SkillTableData.Default();
    }
}
