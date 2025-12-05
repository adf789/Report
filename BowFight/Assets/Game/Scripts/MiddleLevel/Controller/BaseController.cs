using UnityEngine;

public class BaseController : MonoBehaviour
{
    public Archer Archer => _archer;
    public int SkillCount => _skillDatas != null ? _skillDatas.Length : 0;

    [SerializeField] protected Archer _archer;
    [SerializeField] protected Collider2D _collider;
    [SerializeField] protected Rigidbody2D _rigidBody;
    [SerializeField] protected float _shootDelay = 0.3f;
    [SerializeField] protected float _jumpForce = 10f;

    protected SkillTableData[] _skillDatas = null;
    protected float _minX;
    protected float _maxX;
    protected bool _isPause = false;
    private float _shootTime;
    private float _accelation = 0f;
    private float _prevVelocityY = 0f;
    private float _useSkillTime = 0f;
    private float _remainDashTime = 0f;
    private bool _isJumping = false;

    private readonly int ATTACK_FIRST_DELAY = 2;
    private readonly int ACCELATION_WEIGHT = 2;
    private readonly float USE_SKILL_DELAY = 0.5f;
    private readonly float DASH_TIME = 0.5f;

    public virtual void Initialize(SkillTableData[] skillDatas)
    {
        _shootTime = Time.realtimeSinceStartup + ATTACK_FIRST_DELAY;
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

    public SkillTableData GetSkillData(int index)
    {
        if (SkillCount <= index || index < 0)
            return default;

        return _skillDatas[index];
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
        if (Time.realtimeSinceStartup - _shootTime < _shootDelay)
            return;

        _shootTime = Time.realtimeSinceStartup;

        _archer.Shoot();
    }

    protected bool ShootSkillArrow(int index)
    {
        if (!CheckSkillDelay())
            return false;

        var skillData = GetSkillData(index);

        if (skillData == null)
        {
            Debug.LogError($"Not find skill (Index: {index})");
            return false;
        }

        // 일반 공격 지연
        _shootTime = Time.realtimeSinceStartup;

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

        var bounds = _collider.bounds;
        float halfSize = bounds.size.x * 0.5f;

        // 방향 전환 유무
        int flip = _archer.IsFlip ? -1 : 1;

        // 가속력 변화
        _accelation = Mathf.MoveTowards(_accelation, _archer.MoveSpeed * _archer.DashSpeed, _archer.MoveSpeed * _archer.DashSpeed * Time.fixedDeltaTime * ACCELATION_WEIGHT);

        // 앞쪽 범위 초과 체크
        if (!_archer.IsFlip && transform.position.x + halfSize >= _maxX)
        {
            OnAfterDash();
            return true;
        }
        else if (_archer.IsFlip && transform.position.x - halfSize <= _minX)
        {
            OnAfterDash();
            return true;
        }

        // 앞쪽으로 이동
        transform.Translate(-Vector3.left * flip * _accelation * Time.fixedDeltaTime);

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
            _accelation = 0f;

            if (!_isJumping)
                ShootArrow();

            return;
        }

        var bounds = _collider.bounds;
        float halfSize = bounds.size.x * 0.5f;

        // 방향 전환 유무
        int flip = _archer.IsFlip ? -1 : 1;

        // 가속력 변화
        _accelation = Mathf.MoveTowards(_accelation, _archer.MoveSpeed, _archer.MoveSpeed * Time.fixedDeltaTime * ACCELATION_WEIGHT);

        if (_archer.MoveState == MoveState.BackwardMove)
        {
            // 뒤쪽 범위 초과 체크
            if (!_archer.IsFlip && transform.position.x - halfSize <= _minX)
                return;
            else if (_archer.IsFlip && transform.position.x + halfSize >= _maxX)
                return;

            // 뒤쪽으로 이동
            transform.Translate(Vector3.left * flip * _accelation * Time.fixedDeltaTime);
        }
        else if (_archer.MoveState == MoveState.ForwardMove)
        {
            // 앞쪽 범위 초과 체크
            if (!_archer.IsFlip && transform.position.x + halfSize >= _maxX)
                return;
            else if (_archer.IsFlip && transform.position.x - halfSize <= _minX)
                return;

            // 앞쪽으로 이동
            transform.Translate(-Vector3.left * flip * _accelation * Time.fixedDeltaTime);
        }
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
}
