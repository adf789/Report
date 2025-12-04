using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleScene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private AIController _aiPlayer;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _maskCamera;
    [SerializeField] private Camera _viewCamera;

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

        OnEventShowBlind(false);
    }

    void Update()
    {
        // 디버그용
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.isPressed)
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
#endif
    }

    private void Initialize()
    {
        // AI 셋업
        if (_aiPlayer)
        {
            var moveAreaBounds = _aiPlayerLimitMoveArea.bounds;
            var skillDatas = GetRandomSkillDatas();

            _aiPlayer.Archer.SetTarget(_player.Archer);
            _aiPlayer.Archer.SetArrowPool(_arrowPool);
            _aiPlayer.Archer.SetEvents(OnEventUpdateAIPlayerBuffs,
            OnEventUpdateAIPlayerHp,
            null,
            OnEventShowDamage);

            _aiPlayer.Initialize(skillDatas);
            _aiPlayer.SetMoveLimit(moveAreaBounds.min.x, moveAreaBounds.max.x);
        }

        // 플레이어 셋업
        if (_player)
        {
            var moveAreaBounds = _playerLimitMoveArea.bounds;
            var skillDatas = GetRandomSkillDatas();

            _player.Archer.SetTarget(_aiPlayer.Archer);
            _player.Archer.SetArrowPool(_arrowPool);
            _player.Archer.SetEvents(OnEventUpdatePlayerBuffs,
            OnEventUpdatePlayerHp,
            OnEventShowBlind,
            OnEventShowDamage);

            _player.Initialize(skillDatas);
            _player.SetMoveLimit(moveAreaBounds.min.x, moveAreaBounds.max.x);

            // UI 셋업
            if (_battleUI)
            {
                var battleMainUnitModel = new BattleMainUnitModel();

                battleMainUnitModel.SetEvents(_player.RightMove, _player.LeftMove, _player.StopMove);
                battleMainUnitModel.SetSkillDatas(skillDatas, _player.UseSkill);

                _battleUI.SetModel(battleMainUnitModel);
                _battleUI.SetEventBlindTargetPosition(_player.Archer.GetPosition);
                _battleUI.SetCamera(_mainCamera, _viewCamera);

                UpdateUIModelByPlayerHP(true, true);
                UpdateUIModelByPlayerBuffs(true, true);

                _battleUI.Show();
            }
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
                if (!data.IsActive)
                    continue;

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

    private void OnEventUpdatePlayerHp()
    {
        if (!_battleUI)
            return;

        UpdateUIModelByPlayerHP(true, false);

        _battleUI.ShowPlayerStateBar();
    }

    private void OnEventUpdatePlayerBuffs()
    {
        if (!_battleUI)
            return;

        UpdateUIModelByPlayerBuffs(true, false);

        _battleUI.ShowPlayerStateBar();
    }

    private void OnEventShowDamage(Vector3 position, int damage)
    {
        if (!_battleUI)
            return;

        _battleUI.ShowDamage(position, damage);
    }

    private void OnEventUpdateAIPlayerHp()
    {
        if (!_battleUI)
            return;

        UpdateUIModelByPlayerHP(false, true);

        _battleUI.ShowAIPlayerStateBar();
    }

    private void OnEventUpdateAIPlayerBuffs()
    {
        if (!_battleUI)
            return;

        UpdateUIModelByPlayerBuffs(false, true);

        _battleUI.ShowAIPlayerStateBar();
    }

    private void OnEventShowBlind(bool isActive)
    {
        if (_maskCamera)
            _maskCamera.enabled = isActive;

        if (_battleUI)
            _battleUI.SetActiveBlind(isActive);
    }

    private void UpdateUIModelByPlayerHP(bool isPlayer, bool isAIPlayer)
    {
        if (isPlayer)
        {
            _battleUI.Model.PlayerStateBarModel.MaxHp = _player.Archer.MaxHP;
            _battleUI.Model.PlayerStateBarModel.CurrentHp = _player.Archer.CurrentHP;
        }

        if (isAIPlayer)
        {
            _battleUI.Model.AIPlayerStateBarModel.MaxHp = _aiPlayer.Archer.MaxHP;
            _battleUI.Model.AIPlayerStateBarModel.CurrentHp = _aiPlayer.Archer.CurrentHP;
        }
    }

    private void UpdateUIModelByPlayerBuffs(bool isPlayer, bool isAIPlayer)
    {
        if (isPlayer)
        {
            _battleUI.Model.PlayerStateBarModel.ClearAffectedBuffs();

            foreach (var buff in _player.Archer.GetAffectedBuffs())
            {
                _battleUI.Model.PlayerStateBarModel.AddAffectedBuff(buff.Thumbnail);
            }
        }

        if (isAIPlayer)
        {
            _battleUI.Model.AIPlayerStateBarModel.ClearAffectedBuffs();

            foreach (var buff in _aiPlayer.Archer.GetAffectedBuffs())
            {
                _battleUI.Model.AIPlayerStateBarModel.AddAffectedBuff(buff.Thumbnail);
            }
        }
    }
}
