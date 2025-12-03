using System;
using System.Collections.Generic;
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
    private float _shootTime;
    private float _accelation = 0f;
    private float _prevVelocityY = 0f;
    private bool _isJumping = false;
    private Queue<Action> _afterJumpEvents = new();

    private readonly int ATTACK_FIRST_DELAY = 2;
    private readonly int ACCELATION_WEIGHT = 2;

    public virtual void Initialize(SkillTableData[] skillDatas)
    {
        _shootTime = Time.realtimeSinceStartup + ATTACK_FIRST_DELAY;
        _skillDatas = skillDatas;
    }

    public void SetMoveLimit(float minX, float maxX)
    {
        _minX = minX;
        _maxX = maxX;
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

    protected virtual void ShootArrow()
    {
        if (Time.realtimeSinceStartup - _shootTime < _shootDelay)
            return;

        _shootTime = Time.realtimeSinceStartup;

        _archer.Fire();
    }

    protected bool ShootSkillArrow(int index)
    {
        var skillData = GetSkillData(index);

        if (skillData == null)
        {
            Debug.LogError($"Not find skill (Index: {index})");
            return false;
        }

        // 일반 공격 지연
        _shootTime = Time.realtimeSinceStartup;

        // 점프 후 발사하는 경우
        // 최고점에 도달 후 발사하도록 지연 발사를 적용
        if (skillData.IsJump)
        {
            if (skillData.IsDirect)
                _afterJumpEvents.Enqueue(_archer.DirectFire);
            else
                _afterJumpEvents.Enqueue(_archer.Fire);

            Jump();
        }
        // 바로 발사의 경우
        else
        {
            if (skillData.IsDirect)
                _archer.DirectFire();
            else
                _archer.Fire();
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
        OnUpdateSideMove();

        OnCheckSkillCondition();
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

        // 가속력 변화
        _accelation = Mathf.MoveTowards(_accelation, _archer.MoveSpeed, _archer.MoveSpeed * Time.deltaTime * ACCELATION_WEIGHT);

        if (_archer.MoveState == MoveState.LeftMove)
        {
            // 왼쪽 범위 초과 체크
            if (transform.position.x - halfSize <= _minX)
                return;

            // 왼쪽으로 이동
            transform.Translate(Vector3.left * _accelation * Time.deltaTime);
        }
        else if (_archer.MoveState == MoveState.RightMove)
        {
            // 오른쪽 범위 초과 체크
            if (transform.position.x + halfSize >= _maxX)
                return;

            // 오른쪽으로 이동
            transform.Translate(-Vector3.left * _accelation * Time.deltaTime);
        }
    }

    private void OnCheckSkillCondition()
    {
        if (CheckJumpHighestPoint())
        {
            OnAfterJumpEvents();
        }
    }

    private void OnAfterJumpEvents()
    {
        while (_afterJumpEvents.Count > 0)
        {
            var func = _afterJumpEvents.Dequeue();
            func?.Invoke();
        }
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
}
