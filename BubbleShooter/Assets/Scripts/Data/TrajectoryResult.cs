using UnityEngine;

public struct TrajectoryResult
{
    public Vector2[] Points;
    public bool HasReflection;
    public Vector2 FinalPosition;
    public bool HitBubble;
    public RaycastHit2D HitInfo;
}
