using UnityEngine;

public interface ITarget
{
    public int InstanceID { get; }
    public void OnDamaged(Arrow arrow);
    public Vector3 GetPosition();
}
