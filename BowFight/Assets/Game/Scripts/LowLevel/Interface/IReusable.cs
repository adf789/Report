using UnityEngine;

public interface IReusable
{
    public void Initialize();
    public void SetActive(bool isActive);
    public void ReturnToPool();
    public void ResetEvents();
}
