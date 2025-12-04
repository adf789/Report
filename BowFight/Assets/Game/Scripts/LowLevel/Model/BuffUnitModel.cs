using UnityEngine;

public struct BuffUnitModel : IUnitModel
{
    public string Thumbnail { get; private set; }

    public BuffUnitModel(string thumbnail)
    {
        Thumbnail = thumbnail;
    }
}
