using UnityEngine;

public interface IBuffData
{
    public uint ID { get; }
    public BuffType Type { get; }
    public float PerTime { get; }
    public float StartTime { get; }
    public float RemainTime { get; }
    public string Thumbnail { get; }
    public float AddValue { get; }
    public int UpdateTime(float currentTime);
}
