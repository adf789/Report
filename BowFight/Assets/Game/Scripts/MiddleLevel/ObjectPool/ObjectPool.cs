using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour, IReusable
{
    [SerializeField] private T _prefab;
    [SerializeField] private int _initCount;
    private Queue<T> _pool = new();
    private HashSet<T> _checkPool = new();

    public void Initialize()
    {
        for (int i = 0; i < _initCount; i++)
        {
            Add(CreateObject());
        }
    }

    public T CreateObject()
    {
        T newObject = Instantiate(_prefab, transform);

        newObject.Initialize();

        return newObject;
    }

    public void Add(T newObject)
    {
        if (newObject == null)
            return;

        if (_checkPool.Contains(newObject))
            return;

        newObject.ResetEvents();
        newObject.SetActive(false);

        _checkPool.Add(newObject);
        _pool.Enqueue(newObject);
    }

    public T Get()
    {
        T obj = null;

        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
            _checkPool.Remove(obj);
        }
        else
        {
            obj = CreateObject();
            obj.SetActive(false);
        }

        return obj;
    }
}
