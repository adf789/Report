using UnityEngine;

public struct SkillUnitModel : IUnitModel
{
    public bool IsNull { get => DataID == 0; }
    public uint DataID { get; private set; }
    public float CoolTime { get; private set; }
    public bool IsCoolTime { get; private set; }
    public string ThumbnailPath { get; private set; }
    public int RemainCoolTime => Mathf.CeilToInt(CoolTime - _elapsedTime);

    private int _skillIndex;
    private float _elapsedTime;
    private System.Func<int, bool> _onEventUseSkill;

    public SkillUnitModel(SkillTableData data, int index, System.Func<int, bool> onEventUseSkill)
    {
        DataID = data.ID;
        CoolTime = data.CoolTime;
        IsCoolTime = false;
        ThumbnailPath = data.ThumbnailPath;

        _elapsedTime = 0f;
        _skillIndex = index;
        _onEventUseSkill = onEventUseSkill;
    }

    public void PassTime(float time)
    {
        if (!IsCoolTime)
            return;

        _elapsedTime += time;

        if (_elapsedTime >= CoolTime)
        {
            _elapsedTime = CoolTime;
            IsCoolTime = false;
        }
    }

    public bool OnEventUseSkill()
    {
        if (IsCoolTime)
            return false;

        if (_onEventUseSkill == null)
            return false;

        bool isUse = _onEventUseSkill(_skillIndex);

        if (!isUse)
            return false;

        if (CoolTime > 0)
        {
            IsCoolTime = true;
            _elapsedTime = 0f;
        }

        return true;
    }
}
