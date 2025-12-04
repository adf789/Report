using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleScene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private AIController _aiPlayer;

    [Header("UI")]
    [SerializeField] private BattleMainUnit _battleUI;

    [Header("ObjectPool")]
    [SerializeField] private ArrowObjectPool _arrowPool;

    [Header("Limit Move Area")]
    [SerializeField] private BoxCollider2D _playerLimitMoveArea;
    [SerializeField] private BoxCollider2D _aiPlayerLimitMoveArea;

    private SkillTable _skillTable;

    private readonly string PATH_SKILL_TABLE = "Table/SkillTable";
    private readonly int SKILL_COUNT = 5;

    void Awake()
    {
        _arrowPool.Initialize();
    }

    void Start()
    {
        LoadSkillTable();

        Initialize();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.isPressed)
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
#endif
    }

    private void Initialize()
    {
        // 플레이어 셋업
        if (_player)
        {
            var moveAreaBounds = _playerLimitMoveArea.bounds;
            var skillDatas = GetRandomSkillDatas();

            _player.Archer.SetTarget(_aiPlayer.Archer);
            _player.Archer.SetArrowPool(_arrowPool);

            _player.Initialize(skillDatas);
            _player.SetMoveLimit(moveAreaBounds.min.x, moveAreaBounds.max.x);

            // UI 셋업
            if (_battleUI)
            {
                var battleMainUnitModel = new BattleMainUnitModel();

                battleMainUnitModel.SetEvents(_player.RightMove, _player.LeftMove, _player.StopMove);
                battleMainUnitModel.SetSkillDatas(skillDatas, _player.UseSkill);

                _battleUI.SetModel(battleMainUnitModel);
                _battleUI.Show();
            }
        }

        // AI 셋업
        if (_aiPlayer)
        {
            var moveAreaBounds = _aiPlayerLimitMoveArea.bounds;
            var skillDatas = GetRandomSkillDatas();

            _aiPlayer.Archer.SetTarget(_player.Archer);
            _aiPlayer.Archer.SetArrowPool(_arrowPool);

            _aiPlayer.Initialize(skillDatas);
            _aiPlayer.SetMoveLimit(moveAreaBounds.min.x, moveAreaBounds.max.x);
        }
    }

    private void LoadSkillTable()
    {
        _skillTable = Resources.Load<SkillTable>(PATH_SKILL_TABLE);
    }

    private SkillTableData[] GetRandomSkillDatas()
    {
        var skillDatas = new List<SkillTableData>(SKILL_COUNT);
        var addSkills = new HashSet<uint>();

        if (_skillTable == null)
            return skillDatas.ToArray();

        for (int i = 0; i < SKILL_COUNT; i++)
        {
            if (addSkills.Count == _skillTable.GetDataCount())
                break;

            SkillTableData pickData = null;
            int randomValue = 0;
            foreach (var data in _skillTable.GetAllDatas())
            {
                if (addSkills.Contains(data.ID))
                    continue;

                if (Random.Range(0, ++randomValue) == 0)
                    pickData = data;
            }

            if (pickData != null)
            {
                skillDatas.Add(pickData);
                addSkills.Add(pickData.ID);
            }
        }

        return skillDatas.ToArray();
    }
}
