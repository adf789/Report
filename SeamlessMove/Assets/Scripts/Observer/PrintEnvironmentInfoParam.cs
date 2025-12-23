using UnityEngine;

public struct PrintEnvironmentInfoParam : IObserverParam
{
    public string CurrentArea;
    public Vector2Int CurrentGrid;
    public string TargetArea;
    public Vector2Int TargetGrid;
}
