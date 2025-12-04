using UnityEngine;

public struct FreezeDebuffData : IBuffData
{
    public uint ID { get; private set; }
    public BuffType Type { get => BuffType.Freeze; }
    public float PerTime { get => 1f; }
    public float StartTime { get; private set; }
    public float RemainTime { get; private set; }
    public string Thumbnail { get; private set; }
    public float AddValue { get; private set; }

    public FreezeDebuffData(SkillTableData skillData, float value, float currentTime)
    {
        if (skillData == null)
        {
            Debug.LogError("[PoisionDebuffData]: 잘못된 스킬 데이터입니다.");
            skillData = SkillTableData.Default();
        }

        ID = skillData.ID;
        StartTime = currentTime;
        RemainTime = skillData.Duration;
        Thumbnail = skillData.Thumbnail;
        AddValue = value * skillData.DurationDamageRate;
    }

    public int UpdateTime(float currentTime)
    {
        if (RemainTime <= float.Epsilon)
            return 0;

        if (StartTime + PerTime > currentTime)
            return 0;

        float diffTime = Mathf.Max(0, currentTime - StartTime);
        int affectCount = Mathf.FloorToInt(diffTime / PerTime);

        StartTime += PerTime * affectCount;
        RemainTime = Mathf.Max(0, RemainTime - PerTime * affectCount);

        return affectCount;
    }
}
