
using UnityEngine;

public class SkillTableData : BaseTableData
{
    public float DamageRate => _damageRate;
    public float DurationDamageRate => _durationDamageRate;
    public float ArrowMoveSpeed => _arrowMoveSpeed;
    public float Duration => _duration;
    public int CoolTime => _coolTime;
    public int SpawnCount => _spawnCount;
    public float SpawnRadius => _spawnRadius;
    public SkillMoveType MoveType => _moveType;
    public SkillEffectType EffectType => _effectType;
    public SkillTargetType TargetType => _targetType;
    public ArrowMoveType ArrowMoveType => _arrowMoveType;


    [Header("Skill Value Setting")]
    [Tooltip("적용 데미지 비율")]
    [Range(0, 10)]
    [SerializeField] private float _damageRate = 1;

    [Tooltip("지속 데미지 적용 비율")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float _durationDamageRate = 0.1f;

    [Tooltip("속력")]
    [Range(0.5f, 2f)]
    [SerializeField] private float _arrowMoveSpeed = 1;

    [Tooltip("지속시간")]
    [Range(0, 10)]
    [SerializeField] private float _duration;

    [Tooltip("쿨타임")]
    [Range(0, 120)]
    [SerializeField] private int _coolTime;

    [Tooltip("소환 수")]
    [Range(1, 10)]
    [SerializeField] private int _spawnCount = 1;

    [Tooltip("소환 범위")]
    [Range(2, 5)]
    [SerializeField] private float _spawnRadius;

    [Header("Skill Type Setting")]
    [SerializeField] private SkillMoveType _moveType;
    [SerializeField] private SkillEffectType _effectType;
    [SerializeField] private SkillTargetType _targetType;
    [SerializeField] private ArrowMoveType _arrowMoveType;

    private static SkillTableData _defaultData;

    public static SkillTableData Default()
    {
        if (_defaultData == null)
            _defaultData = new SkillTableData();

        return _defaultData;
    }
}
