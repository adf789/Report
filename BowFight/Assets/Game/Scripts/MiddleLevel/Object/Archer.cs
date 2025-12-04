using System;
using System.Collections.Generic;
using UnityEngine;

public class Archer : MonoBehaviour, ITarget
{
    public MoveState MoveState => _moveState;
    public BowLoadState BowLoadState => _archerAnimation.BowState;
    public float MoveSpeed => _moveSpeed;
    public float DecelerationSpeed => _decelerationSpeed;
    public bool IsFlip => _isFlip;
    public int InstanceID => gameObject.GetInstanceID();

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private ArcherAnimation _archerAnimation;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _targetCollider;

    [Header("Values")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _decelerationSpeed = 5f;
    [SerializeField] private bool _isFlip = false;

    private MoveState _moveState;
    private float _moveDirection;

    private ITarget _target;
    private ArrowObjectPool _arrowPool = null;
    private Queue<SkillTableData> _loadSkills = null;

    void Awake()
    {
        _archerAnimation.SetEventCreateArrow(FireArrow);
        _loadSkills = new Queue<SkillTableData>();
    }

    void Update()
    {
        OnUpdateMove();
    }

    public void Shoot()
    {
        _animator.SetTrigger(AnimationState.Shoot.ToString());
    }

    public void ReadySkillShoot(SkillTableData skillData)
    {
        ReloadSkillArrow(skillData);

        CancelBowAnimation();

        _animator.SetTrigger(AnimationState.SkillShootReady.ToString());
    }

    public void SkillShoot()
    {
        _animator.SetTrigger(AnimationState.SkillShoot.ToString());
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

    private void OnUpdateMove()
    {
        // 방향 전환 유무
        int flip = _isFlip ? -1 : 1;

        switch (_moveState)
        {
            case MoveState.LeftMove:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, -1f * flip, _moveSpeed * Time.deltaTime);
                }
                break;

            case MoveState.RightMove:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, 1f * flip, _moveSpeed * Time.deltaTime);
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

    private void ArrowReturnToPool(Arrow arrow)
    {
        _arrowPool?.Add(arrow);
    }

    private Arrow GetArrow(Vector3 position)
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
        arrow.SetPosition(position);
        arrow.SetTarget(_target);

        return arrow;
    }

    private void FireArrow(Vector3 position)
    {
        var arrow = GetArrow(position);

        ArrowMoveType moveType = ArrowMoveType.Parabola;

        if (_loadSkills.Count > 0)
        {
            var skillData = _loadSkills.Dequeue();

            if (skillData.IsDirect)
                moveType = ArrowMoveType.Direct;
        }

        arrow?.StartMove(moveType);
    }

    private void ReloadSkillArrow(SkillTableData skillData)
    {
        _loadSkills.Enqueue(skillData);
    }

    private void CancelBowAnimation()
    {
        if (_archerAnimation.BowState == BowLoadState.Load)
        {
            _archerAnimation.CancelBowAnimation();
        }
    }
}
