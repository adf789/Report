using UnityEngine;

public class BaseController : MonoBehaviour
{
    public Archer Archer => _archer;

    [Header("References")]
    [SerializeField] protected Archer _archer;
    [SerializeField] protected Collider2D _collider;
    [SerializeField] protected Rigidbody2D _rigidBody;

    [Header("Values")]
    [SerializeField] protected float _shootDelay = 1f;
    [SerializeField] protected float _jumpForce = 350f;

    protected SkillTableData[] _skillDatas = null;
    protected float _minX;
    protected float _maxX;
    protected bool _isPause = false;
    private float _shootTime;
    private float _accelation;
    private float _prevVelocityY;
    private float _useSkillTime;
    private float _remainDashTime;
    private bool _isJumping = false;

    private readonly int ATTACK_FIRST_DELAY = 2;
    private readonly int ACCELATION_WEIGHT = 2;
    private readonly float USE_SKILL_DELAY = 0.5f;
    private readonly float DASH_TIME = 0.5f;

    public virtual void Initialize(SkillTableData[] skillDatas)
    {
        _shootTime = Time.time + ATTACK_FIRST_DELAY;
        _skillDatas = skillDatas;

        _archer.Initialize();
    }

    public void SetMoveLimit(float minX, float maxX)
    {
        _minX = minX;
        _maxX = maxX;
    }

    public void SetPause(bool isPause)
    {
        _isPause = isPause;
    }

    protected void Jump()
    {
        if (_isJumping)
            return;

        _isJumping = true;
        _prevVelocityY = 0;

        _rigidBody.AddForceY(_jumpForce);
        _archer.Jump();
    }

    protected void Dash()
    {
        _remainDashTime = DASH_TIME;

        _archer.Move(MoveState.ForwardDash);
    }

    protected virtual void ShootArrow()
    {
        if (Time.time - _shootTime < _shootDelay)
            return;

        _shootTime = Time.time;

        _archer.Shoot();
    }

    protected bool ShootSkillArrow(int index)
    {
        if (!CheckSkillDelay())
            return false;

        if (CheckMoveMotion())
            return false;

        var skillData = GetSkillData(index);

        if (skillData == null)
        {
            Debug.LogError($"Not find skill (Index: {index})");
            return false;
        }

        // 일반 공격 지연
        _shootTime = Time.time;

        // 스킬 준비
        _archer.ReadySkillShoot(skillData);

        // 점프 후 발사하는 경우
        // 최고점에 도달 후 발사하도록 지연 발사를 적용
        if (skillData.MoveType == SkillMoveType.Jump)
        {
            Jump();
        }
        else if (skillData.MoveType == SkillMoveType.Dash)
        {
            Dash();
        }
        else
        {
            _archer.SkillShoot();
        }

        return true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ground 레이어와 충돌 시 착지
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 점프 중 착지 감지
            if (_isJumping)
            {
                _archer.JumpLand();
                _isJumping = false;
            }
        }
    }

    protected void OnUpdateMove()
    {
        if (_isPause)
            return;

        if (!OnUpdateDash())
            OnUpdateSideMove();

        OnCheckSkillCondition();
    }

    private bool OnUpdateDash()
    {
        if (_remainDashTime <= float.Epsilon)
            return false;

        // 가속력 변화
        AddAccelation(_archer.DashSpeed);

        // 범위 초과 체크
        if (CheckMoveLimit(_archer.MoveState))
        {
            OnAfterDash();
            return true;
        }

        // 이동 반영
        MovePosition(_archer.MoveState);

        // 대쉬 남은 시간 체크
        _remainDashTime = Mathf.Max(0, _remainDashTime - Time.fixedDeltaTime);
        if (_remainDashTime <= float.Epsilon)
            OnAfterDash();

        return true;
    }

    private void OnUpdateSideMove()
    {
        // 정지
        if (_archer.MoveState == MoveState.None)
        {
            ResetAccelation();

            if (!_isJumping)
                ShootArrow();

            return;
        }

        // 가속력 변화
        AddAccelation();

        // 제한범위내 이동 반영
        if (!CheckMoveLimit(_archer.MoveState))
            MovePosition(_archer.MoveState);
    }

    private void OnCheckSkillCondition()
    {
        if (CheckJumpHighestPoint())
        {
            OnAfterJump();
        }
    }

    private void OnAfterJump()
    {
        _archer.SkillShoot();
    }

    private void OnAfterDash()
    {
        _remainDashTime = 0;

        _archer.ImmediateStopMove();
        _archer.SkillShoot();
    }

    private void AddAccelation(float weight = 1)
    {
        float speed = _archer.MoveSpeed * weight;
        float delta = _archer.MoveSpeed * weight * ACCELATION_WEIGHT * Time.fixedDeltaTime;

        _accelation = Mathf.MoveTowards(_accelation, speed, delta);
    }

    private void ResetAccelation()
    {
        _accelation = 0;
    }

    private void MovePosition(MoveState moveState)
    {
        int flip = _archer.IsFlip ? -1 : 1;
        float animationSpeed = _archer.AnimationSpeed;
        float translateDelta = flip * _accelation * animationSpeed * Time.fixedDeltaTime;

        switch (moveState)
        {
            case MoveState.ForwardMove:
            case MoveState.ForwardDash:
                {
                    transform.Translate(-Vector3.left * translateDelta);
                }
                break;

            case MoveState.BackwardMove:
                {
                    transform.Translate(Vector3.left * translateDelta);
                }
                break;
        }
    }

    protected SkillTableData GetSkillData(int index)
    {
        if (_skillDatas == null)
            return SkillTableData.Default();

        if (_skillDatas.Length <= index || index < 0)
            return SkillTableData.Default();

        return _skillDatas[index];
    }

    private bool CheckMoveLimit(MoveState moveState)
    {
        // 이동 범위 offset
        var bounds = _collider.bounds;
        float halfSize = bounds.size.x * 0.5f;

        // 이동 타입별 범위 초과 체크
        switch (moveState)
        {
            case MoveState.ForwardMove:
            case MoveState.ForwardDash:
                {
                    if (!_archer.IsFlip && transform.position.x + halfSize >= _maxX)
                        return true;
                    else if (_archer.IsFlip && transform.position.x - halfSize <= _minX)
                        return true;
                }
                return false;

            case MoveState.BackwardMove:
                {
                    if (!_archer.IsFlip && transform.position.x - halfSize <= _minX)
                        return true;
                    else if (_archer.IsFlip && transform.position.x + halfSize >= _maxX)
                        return true;
                }
                return false;
        }

        return true;
    }

    private bool CheckJumpHighestPoint()
    {
        if (!_isJumping)
            return false;

        if (_rigidBody.totalForce.y > 0)
            return false;

        bool change = _prevVelocityY > 0 && _rigidBody.linearVelocityY <= 0;

        _prevVelocityY = _rigidBody.linearVelocityY;

        return change;
    }

    private bool CheckSkillDelay()
    {
        if (_useSkillTime + USE_SKILL_DELAY > Time.realtimeSinceStartup)
            return false;

        _useSkillTime = Time.realtimeSinceStartup;

        return true;
    }

    private bool CheckMoveMotion()
    {
        return _isJumping || _archer.MoveState == MoveState.ForwardDash;
    }
}
