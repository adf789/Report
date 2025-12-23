using UnityEngine;

public struct PrintPlayerInfoParam : IObserverParam
{
    public MoveState MoveState;
    public Vector3 CurrentPosition;
    public Vector3 DestinationPosition;
    public float LeftDistance => Vector3.Distance(CurrentPosition, DestinationPosition);
}
