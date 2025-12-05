using System;
using System.Collections.Generic;
using UnityEngine;

public class Archer : MonoBehaviour, ITarget
{
    public MoveState MoveState => _moveState;
    public float MoveSpeed => _moveSpeed;
    public float DashSpeed => _dashSpeed;
    public float MaxHP => _hp;
    public float CurrentHP => _currentHp;
    public float AnimationSpeed => _archerAnimation.Speed;
    public bool IsFlip => _isFlip;
    public int LoadSkillCount => _loadSkills != null ? _loadSkills.Count : 0;
    public int InstanceID => gameObject.GetInstanceID();

    [Header("References")]
    [SerializeField] private ArcherAnimation _archerAnimation;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _targetCollider;

    [Header("Values")]
    [SerializeField] private float _hp = 100f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashSpeed = 5f;
    [SerializeField] private float _decelerationSpeed = 5f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private bool _isFlip = false;

    private MoveState _moveState;
    private CrowdControlState _crowdControlState;
    private float _moveDirection;
    private float _currentHp;
    private float _updateBuffTime;

    private ITarget _target;
    private ArrowObjectPool _arrowPool = null;
    private Queue<SkillTableData> _loadSkills = null;
    private Queue<IBuffData> _affectedBuffs = null;

    private Action _onEventUpdateBuffUI = null;
    private Action _onEventUpdateHp = null;
    private Action<bool> _onEventBlind = null;
    private Action<Vector3, int, SkillEffectType> _onEventShowDamage = null;

    private readonly float UPDATE_BUFF_TIME = 0.1f;

    void Awake()
    {
        _archerAnimation.SetEventCreateArrow(FireArrow);
        _loadSkills = new Queue<SkillTableData>();
        _affectedBuffs = new Queue<IBuffData>();
    }

    void Update()
    {
        OnUpdateMove();

        OnUpdateBuff();
    }

    #region SET AND MODIFY VALUES
    public void Initialize()
    {
        _currentHp = _hp;
    }

    public void SetEvents(Action onEventUpdateBuffUI,
    Action onEventUpdateHp,
    Action<bool> onEventBlind,
    Action<Vector3, int, SkillEffectType> onEventShowDamage)
    {
        _onEventUpdateBuffUI = onEventUpdateBuffUI;
        _onEventUpdateHp = onEventUpdateHp;
        _onEventBlind = onEventBlind;
        _onEventShowDamage = onEventShowDamage;
    }

    public void SetArrowPool(ArrowObjectPool arrowPool)
    {
        _arrowPool = arrowPool;
    }

    public void SetTarget(ITarget target)
    {
        _target = target;
    }

    private void AddCrowdControl(CrowdControlState state)
    {
        _crowdControlState |= state;

        if (state == CrowdControlState.Shock)
            Move(MoveState.None);
        else if (state == CrowdControlState.Freeze)
            _archerAnimation.SetSpeed(0.3f);
    }

    private void RemoveCrowdControl(CrowdControlState state)
    {
        _crowdControlState &= ~state;

        if (state == CrowdControlState.Freeze)
            _archerAnimation.SetSpeed(1f);
    }

    private void AddHp(float value)
    {
        if (value == 0)
            return;

        _currentHp = Mathf.Max(0, _currentHp + value);

        _onEventUpdateHp?.Invoke();
    }

    private void ApplySelfSkill(SkillTableData skillData)
    {
        for (int i = 0; i < skillData.SpawnCount; i++)
        {
            float damage = _attackDamage * skillData.DamageRate / skillData.SpawnCount;

            OnApplyArrowEffect(GetPosition(), damage, skillData);
        }
    }
    #endregion SET VALUES

    #region GET VALUE
    public Vector3 GetPosition()
    {
        return _targetCollider ? _targetCollider.bounds.center : transform.position;
    }

    public float GetMinSpawnHeight()
    {
        return _targetCollider ? _targetCollider.bounds.min.y : 0;
    }

    public IEnumerable<IBuffData> GetAffectedBuffs()
    {
        foreach (var buff in _affectedBuffs)
        {
            yield return buff;
        }
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

        arrow.SetEventReturnToPool(OnEventArrowReturnToPool);
        arrow.SetPosition(position);

        return arrow;
    }

    private IBuffData GetBuffData(SkillTableData skillData)
    {
        if (skillData == null)
            return null;

        switch (skillData.EffectType)
        {
            case SkillEffectType.Fire:
                return new BurningDebuffData(skillData, _attackDamage, Time.time);

            case SkillEffectType.Poison:
                return new PoisonDebuffData(skillData, _attackDamage, Time.time);

            case SkillEffectType.Ice:
                return new FreezeDebuffData(skillData, _attackDamage, Time.time);

            case SkillEffectType.Lightning:
                return new ShockDebuffData(skillData, _attackDamage, Time.time);

            case SkillEffectType.Dark:
                return new BlindDebuffData(skillData, _attackDamage, Time.time);

            case SkillEffectType.Heal:
                return new HealBuffData(skillData, _attackDamage, Time.time);

            default:
                return null;
        }
    }

    private Vector3 GetRandomPosition()
    {
        var bounds = _targetCollider.bounds;

        float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float y = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);

        return new Vector3(x, y, 0);
    }
    #endregion GET VALUE

    #region CHECK CONDITION
    private bool CheckAlreadyAffectedBuff(uint id)
    {
        foreach (var buff in _affectedBuffs)
        {
            if (buff.ID == id)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region MOVE
    public void Move(MoveState state)
    {
        _moveState = state;

        // 쇼크 상태면 움직일 수 없게
        if (_crowdControlState.HasFlag(CrowdControlState.Shock))
            _moveState = MoveState.None;
    }

    public void ImmediateStopMove()
    {
        Move(MoveState.None);

        _moveDirection = 0;
        _archerAnimation.SetFloat(AnimationParameter.MoveDirection, _moveDirection);
    }

    public void Jump()
    {
        _archerAnimation.SetBool(AnimationParameter.Jump, true);
    }

    public void JumpLand()
    {
        _archerAnimation.SetBool(AnimationParameter.Jump, false);
    }
    #endregion MOVE

    #region SHOOT ARROW
    public void Shoot()
    {
        _archerAnimation.SetTrigger(AnimationParameter.Shoot);
    }

    public void ReadySkillShoot(SkillTableData skillData)
    {
        if (skillData.TargetType == SkillTargetType.Self)
        {
            ApplySelfSkill(skillData);
        }
        else
        {
            ReloadSkillArrow(skillData);

            CancelBowAnimation();

            _archerAnimation.SetTrigger(AnimationParameter.SkillShootReady);
        }
    }

    public void SkillShoot()
    {
        _archerAnimation.SetTrigger(AnimationParameter.SkillShoot);
    }
    #endregion SHOOT ARROW

    #region ANIMATION
    private void CancelBowAnimation()
    {
        if (_archerAnimation.BowState == BowLoadState.Load)
        {
            _archerAnimation.CancelBowAnimation();
        }
    }
    #endregion

    #region CALLBACK EVENT
    public void OnDamaged(Arrow arrow)
    {
        if (arrow == null)
            return;

        OnApplyArrowEffect(arrow.transform.position, arrow.Damage, arrow.SkillData);
    }

    private void OnApplyArrowEffect(Vector3 position, float damage, SkillTableData skillData)
    {
        // 데미지 적용
        AddHp(-damage);
        _onEventShowDamage?.Invoke(position, (int)damage, SkillEffectType.Damage);

        // 버프 적용
        if (skillData != null && skillData.Duration > 0)
        {
            // 이미 적용된 버프인지 확인
            if (!CheckAlreadyAffectedBuff(skillData.ID))
            {
                var newBuff = GetBuffData(skillData);

                if (newBuff != null)
                {
                    OnBuff(newBuff, 0);

                    _affectedBuffs.Enqueue(newBuff);
                }

                _onEventUpdateBuffUI?.Invoke();
            }
        }
    }

    private void OnUpdateMove()
    {
        switch (_moveState)
        {
            case MoveState.BackwardMove:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, -1f, _moveSpeed * Time.deltaTime);
                }
                break;

            case MoveState.ForwardMove:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, 1f, _moveSpeed * Time.deltaTime);
                }
                break;

            case MoveState.ForwardDash:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, 2f, _moveSpeed * _dashSpeed * Time.deltaTime);
                }
                break;

            case MoveState.None:
                {
                    _moveDirection = Mathf.MoveTowards(_moveDirection, 0f, _decelerationSpeed * Time.deltaTime);
                }
                break;
        }

        _archerAnimation.SetFloat(AnimationParameter.MoveDirection, _moveDirection);
    }

    private void OnUpdateBuff()
    {
        int buffCount = _affectedBuffs.Count;
        float currentTime = Time.time;

        if (buffCount == 0)
            return;

        if (_updateBuffTime + UPDATE_BUFF_TIME > currentTime)
            return;

        _updateBuffTime = currentTime;

        for (int i = 0; i < buffCount; i++)
        {
            var buff = _affectedBuffs.Dequeue();
            int appliedValueCount = buff.UpdateTime(currentTime);

            OnBuff(buff, appliedValueCount);

            if (buff.RemainTime > float.Epsilon)
                _affectedBuffs.Enqueue(buff);
        }

        // 버프 수가 변경된 경우
        if (buffCount != _affectedBuffs.Count)
        {
            _onEventUpdateBuffUI?.Invoke();
        }
    }

    private void OnBuff(IBuffData buff, int count)
    {
        if (buff == null)
            return;

        float affectValue = buff.AddValue * count;
        bool isEndTime = buff.RemainTime <= float.Epsilon;

        switch (buff.Type)
        {
            case BuffType.Burning:
            case BuffType.Poison:
                {
                    if (affectValue > 0)
                    {
                        AddHp(-affectValue);

                        var position = GetRandomPosition();
                        var effectType = buff.Type == BuffType.Burning ? SkillEffectType.Fire : SkillEffectType.Poison;
                        _onEventShowDamage?.Invoke(position, (int)affectValue, effectType);
                    }
                }
                break;

            case BuffType.Freeze:
                {
                    if (isEndTime)
                        RemoveCrowdControl(CrowdControlState.Freeze);
                    else
                        AddCrowdControl(CrowdControlState.Freeze);
                }
                break;

            case BuffType.Shock:
                {
                    if (isEndTime)
                        RemoveCrowdControl(CrowdControlState.Shock);
                    else
                        AddCrowdControl(CrowdControlState.Shock);
                }
                break;

            case BuffType.Blind:
                {
                    _onEventBlind?.Invoke(!isEndTime);
                }
                break;

            case BuffType.Heal:
                {
                    if (affectValue > 0)
                    {
                        AddHp(affectValue);

                        var position = GetRandomPosition();
                        _onEventShowDamage?.Invoke(position, (int)affectValue, SkillEffectType.Heal);
                    }
                }
                break;
        }
    }

    private void OnEventArrowReturnToPool(Arrow arrow)
    {
        _arrowPool?.Add(arrow);
    }
    #endregion CALLBACK EVENT

    #region ARROW FUNCTION
    private void FireArrow(Vector3 position)
    {
        if (_loadSkills.Count > 0)
        {
            var skillData = _loadSkills.Dequeue();

            for (int i = 0; i < skillData.SpawnCount; i++)
            {
                // 화살 데미지 설정
                float damage = _attackDamage * skillData.DamageRate / skillData.SpawnCount;

                // 화살 위치 설멍
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * skillData.SpawnRadius;
                Vector3 newPosition = position + (Vector3)randomCircle;
                float minHeight = GetMinSpawnHeight();

                // 화살 최소 높이 설정
                newPosition.y = Mathf.Max(newPosition.y, minHeight);

                // 화살 이동 시작
                StartArrowMove(skillData, newPosition, damage);
            }
        }
        else
        {
            StartArrowMove(SkillTableData.Default(), position, _attackDamage);
        }
    }

    private void StartArrowMove(SkillTableData skillData, Vector3 position, float damage)
    {
        var arrow = GetArrow(position);

        if (arrow == null)
            return;

        arrow.SetTarget(skillData.TargetType == SkillTargetType.Self ? this : _target);
        arrow.SetDamage(damage);
        arrow.StartMove(skillData);
    }

    private void ReloadSkillArrow(SkillTableData skillData)
    {
        _loadSkills.Enqueue(skillData);
    }
    #endregion ARROW FUNCTION
}
